Shader "Hidden/Ocean" {
    Properties {
        _OceanAmbient ("Ocean Ambient", Range(0,1)) = 0.15
    }
    SubShader {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZTest Always
        ZWrite Off
        Cull Off

        Pass {
            Name "OceanPostProcess"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            #include "../Includes/Math.cginc"
            #include "../Includes/Triplanar.cginc"

            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            TEXTURE2D(waveNormalA);
            SAMPLER(sampler_waveNormalA);
            TEXTURE2D(waveNormalB);
            SAMPLER(sampler_waveNormalB);

            float4 colA;
            float4 colB;
            float4 specularCol;
            float depthMultiplier;
            float alphaMultiplier;
            float smoothness;
            float _OceanAmbient;
            float waveStrength;
            float waveNormalScale;
            float waveSpeed;
            float planetScale;
            float3 oceanCentre;
            float oceanRadius;
            float3 dirToSun;

            struct Attributes { uint vertexID : SV_VertexID; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings Vert(Attributes input) {
                Varyings o;
                o.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                o.uv = GetFullScreenTriangleTexCoord(input.vertexID);
                return o;
            }

            float4 Frag(Varyings input) : SV_Target {
                float2 uv = input.uv;
                float4 originalCol = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv);

                float rawDepth = SampleSceneDepth(uv);
                bool isSky =
                #if UNITY_REVERSED_Z
                    rawDepth <= 0.00001;
                #else
                    rawDepth >= 0.99999;
                #endif

                float3 rayPos = GetCameraPositionWS();
                float3 worldPosFar =
                #if UNITY_REVERSED_Z
                    ComputeWorldSpacePosition(uv, 0.0, UNITY_MATRIX_I_VP);
                #else
                    ComputeWorldSpacePosition(uv, 1.0, UNITY_MATRIX_I_VP);
                #endif

                float3 rayDir = normalize(worldPosFar - rayPos);

                float sceneDepth;
                if (!isSky) {
                    float3 sceneWorldPos = ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP);
                    sceneDepth = max(0.0, dot(sceneWorldPos - rayPos, rayDir));
                } else sceneDepth = 1.0e6;

                float2 hitInfo = raySphere(oceanCentre, oceanRadius, rayPos, rayDir);
                float dstToOcean = hitInfo.x;
                float dstThroughOcean = hitInfo.y;

                float dLand = sceneDepth - dstToOcean;
                float eps = max(0.001, 0.001 * sceneDepth);
                if (dLand <= eps) return originalCol;

                float oceanViewDepth = min(dstThroughOcean, dLand);
                if (oceanViewDepth <= 0) return originalCol;

                float3 hitPosWS = rayPos + rayDir * dstToOcean;
                float3 oceanNormal = normalize(hitPosWS - oceanCentre);

                float t = 1 - exp(-oceanViewDepth / planetScale * depthMultiplier);
                float alpha = saturate(1 - exp(-oceanViewDepth / planetScale * alphaMultiplier));
                float4 oceanCol = lerp(colA, colB, t);

                float time = _Time.y;
                float2 waveOffsetA = float2(time * waveSpeed, time * waveSpeed * 0.8);
                float2 waveOffsetB = float2(-time * waveSpeed * 0.8, -time * waveSpeed * 0.3);

                float3 waveNormal = triplanarNormal(hitPosWS - oceanCentre, oceanNormal, waveNormalScale / planetScale, waveOffsetA, TEXTURE2D_ARGS(waveNormalA, sampler_waveNormalA));
                waveNormal = triplanarNormal(hitPosWS - oceanCentre, waveNormal, waveNormalScale / planetScale, waveOffsetB, TEXTURE2D_ARGS(waveNormalB, sampler_waveNormalB));
                waveNormal = normalize(lerp(oceanNormal, waveNormal, waveStrength));

                float3 L = normalize(dirToSun);
                float lambert = saturate(dot(oceanNormal, L));
                float diffuseLighting = max(_OceanAmbient, lambert);

                float3 H = normalize(L - rayDir);
                float specAngle = saturate(dot(H, waveNormal));
                float specular = pow(specAngle, 1.0 / max(0.001, 1 - smoothness));

                oceanCol.rgb *= diffuseLighting;
                oceanCol.rgb += specular * specularCol.rgb;

                return lerp(originalCol, oceanCol, alpha);
            }
            ENDHLSL
        }
    }
}