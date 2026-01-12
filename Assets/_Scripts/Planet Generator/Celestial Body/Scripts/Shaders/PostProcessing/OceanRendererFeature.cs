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
            // Wa¿ne: Renderujemy przed post-processingiem Unity
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            material = OceanEffect.CurrentOceanMaterial;
            if (material == null) return;

            // Pobieramy dane kamery i zasobów
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            TextureHandle source = resourceData.activeColorTexture;

            // 1. POPRAWKA: Tworzenie TextureDesc (zamiast RenderTextureDescriptor)
            var cameraDesc = cameraData.cameraTargetDescriptor;
            TextureDesc desc = new TextureDesc(cameraDesc.width, cameraDesc.height);
            desc.colorFormat = cameraDesc.graphicsFormat;
            desc.depthBufferBits = DepthBits.None;
            desc.msaaSamples = MSAASamples.None;
            desc.name = "_TempOceanTexture";

            TextureHandle tempTexture = renderGraph.CreateTexture(desc);

            // 2. POPRAWKA: U¿ywamy AddUnsafePass, aby mieæ dostêp do CommandBuffer
            using (var builder = renderGraph.AddUnsafePass<PassData>("Ocean Effect Pass", out var passData))
            {
                passData.material = material;
                passData.source = source;
                passData.tempTexture = tempTexture;

                // Deklarujemy zale¿noœci
                builder.UseTexture(source, AccessFlags.ReadWrite);
                builder.UseTexture(tempTexture, AccessFlags.ReadWrite);

                builder.SetRenderFunc((PassData data, UnsafeGraphContext context) =>
                {
                    // 3. POPRAWKA: Konwersja na natywny CommandBuffer
                    CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);

                    // Krok 1: Kamera -> Temp (z materia³em Oceanu)
                    // Blitter.BlitCameraTexture automatycznie ustawia _BlitTexture w shaderze
                    Blitter.BlitCameraTexture(cmd, data.source, data.tempTexture, data.material, 0);

                    // Krok 2: Temp -> Kamera (kopia powrotna)
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