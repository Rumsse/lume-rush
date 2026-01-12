Shader "Hidden/Ocean"
{
    Properties
    {
        _OceanAmbient ("Ocean Ambient", Range(0,1)) = 0.15
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "OceanPostProcess"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            // --- BIBLIOTEKI URP ---
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            // --- TWOJE BIBLIOTEKI (Upewnij się, że ścieżki są poprawne!) ---
            #include "../Includes/Math.cginc"
            #include "../Includes/Triplanar.cginc"

            // --- TEKSTURY ---
            // W Unity 6 Blitter zawsze używa tej nazwy dla wejścia:
            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            TEXTURE2D(waveNormalA);
            SAMPLER(sampler_waveNormalA);

            TEXTURE2D(waveNormalB);
            SAMPLER(sampler_waveNormalB);

            // --- ZMIENNE (UNIFORMY) ---
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

            // --- STRUKTURY ---
            // Używamy VertexID, bo Blitter rysuje proceduralny trójkąt
            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // --- VERTEX SHADER ---
            Varyings Vert(Attributes input)
            {
                Varyings output;
                // Magiczna funkcja URP generująca trójkąt na cały ekran
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }

            // --- FRAGMENT SHADER ---
            float4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                // 1. Pobierz oryginalny obraz gry
                float4 originalCol = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv);

                // 2. Odtwórz pozycję i kierunek w świecie (To naprawia czarny ekran!)
                float3 rayPos = GetCameraPositionWS();
                
                // Rekonstrukcja pozycji świata na podstawie UV i głębi (Far Plane)
                // To zastępuje stare 'i.viewDirWS' z Vertex shadera
                float3 worldPosAtFarPlane = ComputeWorldSpacePosition(uv, 0.0, UNITY_MATRIX_I_VP); // 0.0 = Far plane w Reverse-Z
                float3 rayDir = normalize(worldPosAtFarPlane - rayPos);

                // 3. Pobierz głębię sceny
                float rawDepth = SampleSceneDepth(uv);
                float sceneDepth = LinearEyeDepth(rawDepth, _ZBufferParams);

                // 4. Logika Oceanu (Twoja oryginalna matematyka)
                float2 hitInfo = raySphere(oceanCentre, oceanRadius, rayPos, rayDir);
                float dstToOcean = hitInfo.x;
                float dstThroughOcean = hitInfo.y;

                float oceanViewDepth = min(dstThroughOcean, sceneDepth - dstToOcean);

                if (oceanViewDepth > 0)
                {
                    float3 hitPosWS = rayPos + rayDir * dstToOcean;
                    float3 oceanNormal = normalize(hitPosWS - oceanCentre);

                    float t = 1 - exp(-oceanViewDepth / planetScale * depthMultiplier);
                    float alpha = 1 - exp(-oceanViewDepth / planetScale * alphaMultiplier);

                    float4 oceanCol = lerp(colA, colB, t);

                    // Fale
                    float time = _Time.y;
                    float2 waveOffsetA = float2(time * waveSpeed, time * waveSpeed * 0.8);
                    float2 waveOffsetB = float2(-time * waveSpeed * 0.8, -time * waveSpeed * 0.3);

                    float3 waveNormal = triplanarNormal(
                        hitPosWS - oceanCentre,
                        oceanNormal,
                        waveNormalScale / planetScale,
                        waveOffsetA,
                        TEXTURE2D_ARGS(waveNormalA, sampler_waveNormalA)
                    );

                    waveNormal = triplanarNormal(
                        hitPosWS - oceanCentre,
                        waveNormal,
                        waveNormalScale / planetScale,
                        waveOffsetB,
                        TEXTURE2D_ARGS(waveNormalB, sampler_waveNormalB)
                    );

                    waveNormal = normalize(lerp(oceanNormal, waveNormal, waveStrength));

                    // Oświetlenie
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

                return originalCol;
            }
            ENDHLSL
        }
    }
}