using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class OceanRendererFeature : ScriptableRendererFeature
{
    class OceanRenderPass : ScriptableRenderPass
    {
        private class PassData
        {
            public Material material;
            public TextureHandle source;
            public TextureHandle tempTexture;
        }

        private Material material;

        public OceanRenderPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            material = OceanEffect.CurrentOceanMaterial;
            if (material == null) return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            TextureHandle source = resourceData.activeColorTexture;

            var cameraDesc = cameraData.cameraTargetDescriptor;
            TextureDesc desc = new TextureDesc(cameraDesc.width, cameraDesc.height);
            desc.colorFormat = cameraDesc.graphicsFormat;
            desc.depthBufferBits = DepthBits.None;
            desc.msaaSamples = MSAASamples.None;
            desc.name = "_TempOceanTexture";

            TextureHandle tempTexture = renderGraph.CreateTexture(desc);

            using (var builder = renderGraph.AddUnsafePass<PassData>("Ocean Effect Pass", out var passData))
            {
                passData.material = material;
                passData.source = source;
                passData.tempTexture = tempTexture;

                builder.UseTexture(source, AccessFlags.ReadWrite);
                builder.UseTexture(tempTexture, AccessFlags.ReadWrite);

                builder.SetRenderFunc((PassData data, UnsafeGraphContext context) =>
                {
                    CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);

                    Blitter.BlitCameraTexture(cmd, data.source, data.tempTexture, data.material, 0);

                    Blitter.BlitCameraTexture(cmd, data.tempTexture, data.source);
                });
            }
        }
    }

    OceanRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new OceanRenderPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}