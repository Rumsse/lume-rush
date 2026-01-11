Shader "Celestial/Alien"
{
    Properties
    {
        [Header(Flat Terrain)]
        _ShoreCol("Shore Colour", Color) = (0,0,0,1)
        _FlatColA("Flat Colour A", Color) = (0,0,0,1)
        _FlatColA2("Flat Colour A2", Color) = (0,0,0,1)
        _FlatColB("Flat Colour B", Color) = (0,0,0,1)
        _FlatColB2("Flat Colour B2", Color) = (0,0,0,1)
        _FlatColBlend("Colour Blend", Range(0,3)) = 1.5
        _FlatColBlendNoise("Blend Noise", Range(0,1)) = 0.3
        _ShoreHeight("Shore Height", Range(0,0.2)) = 0.05
        _ShoreBlend("Shore Blend", Range(0,0.2)) = 0.03
        _MaxFlatHeight("Max Flat Height", Range(0,1)) = 0.5

        [Header(Steep Terrain)]
        _SteepColA("Steep Colour A", Color) = (0,0,0,1)
        _SteepColB("Steep Colour B", Color) = (0,0,0,1)
        _SteepBands("Steep Bands", Range(1, 20)) = 8
        _SteepBandStrength("Band Strength", Range(-1,1)) = 0.5

        [Header(Flat to Steep Transition)]
        _SteepnessThreshold("Steep Threshold", Range(0,1)) = 0.5
        _FlatToSteepBlend("Flat to Steep Blend", Range(0,1)) = 0.1
        _FlatToSteepNoise("Flat to Steep Noise", Range(0,1)) = 0.1

        [Header(Noise)]
        [NoScaleOffset] _NoiseTex ("Noise Texture", 2D) = "white" {}
        _NoiseScale("Noise Scale", Float) = 1

        [Header(Other)]
        _FresnelCol("Fresnel Colour", Color) = (1,1,1,1)
        _FresnelStrengthNear("Fresnel Strength Min", float) = 2
        _FresnelStrengthFar("Fresnel Strength Max", float) = 5
        _FresnelPow("Fresnel Power", float) = 2
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _GlossinessFlat ("Smoothness Flat", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _TestParams ("Test Params", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Macros.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "../Includes/Triplanar.cginc"
            #include "../Includes/Math.cginc"

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
            float4 _ShoreCol,_FlatColA,_FlatColA2,_FlatColB,_FlatColB2;
            float4 _SteepColA,_SteepColB,_FresnelCol,_TestParams;

            float _FlatColBlend,_FlatColBlendNoise,_ShoreHeight,_ShoreBlend,_MaxFlatHeight;
            float _SteepBands,_SteepBandStrength;
            float _SteepnessThreshold,_FlatToSteepBlend,_FlatToSteepNoise;

            float _NoiseScale;
            float _Glossiness,_GlossinessFlat,_Metallic;
            float _FresnelStrengthNear,_FresnelStrengthFar,_FresnelPow;

            float2 heightMinMax;
            float oceanLevel;
            float bodyScale;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float4 texcoord   : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 positionOS : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
                float4 tangentOS  : TEXCOORD3;
                float4 terrain    : TEXCOORD4;
                float  fresnel    : TEXCOORD5;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionOS = v.positionOS.xyz;
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.tangentOS = v.tangentOS;
                o.terrain = v.texcoord;

                float3 bodyCentreWS = TransformObjectToWorld(float3(0,0,0));
                float camRadiiFromSurface = (distance(bodyCentreWS, _WorldSpaceCameraPos) - bodyScale) / bodyScale;
                float fresnelT = saturate(camRadiiFromSurface);
                float3 viewDir = normalize(o.positionWS - _WorldSpaceCameraPos);
                float fresStrength = lerp(_FresnelStrengthNear,_FresnelStrengthFar,fresnelT);
                o.fresnel = saturate(fresStrength * pow(1 + dot(viewDir, normalize(o.normalWS)), _FresnelPow));
                return o;
            }

            float4 TriplanarSample(Texture2D tex, SamplerState samp, float3 position, float3 normal, float scale)
            {
                float2 uvX = position.zy * scale;
                float2 uvY = position.xz * scale;
                float2 uvZ = position.xy * scale;

                float4 colX = SAMPLE_TEXTURE2D(tex, samp, uvX);
                float4 colY = SAMPLE_TEXTURE2D(tex, samp, uvY);
                float4 colZ = SAMPLE_TEXTURE2D(tex, samp, uvZ);

                float3 w = normal * normal;
                w /= dot(w, float3(1,1,1));
                return colX * w.x + colY * w.y + colZ * w.z;
            }

            float3 ViewDirWS(float3 posWS) { return normalize(GetWorldSpaceViewDir(posWS)); }

            void Celestial_InitializeInputData(float3 pWS, float3 nWS, float3 vWS, out InputData input)
            {
                ZERO_INITIALIZE(InputData, input);
                input.positionWS = pWS;
                input.normalWS = normalize(nWS);
                input.viewDirectionWS = normalize(vWS);
                input.shadowCoord = float4(0,0,0,0);
                input.fogCoord = 0;
                input.bakedGI = float3(0,0,0);
            }

            float4 frag(Varyings i) : SV_Target
            {
                float3 N = normalize(i.normalWS);
                float3 V = ViewDirWS(i.positionWS);

                float3 sphereNormal = normalize(i.positionOS);
                float steepness = 1 - dot(sphereNormal, N);
                steepness = remap01(steepness, 0, 0.65);

                float terrainHeight = length(i.positionOS);
                float shoreHeight = lerp(heightMinMax.x, 1, oceanLevel);
                float aboveShore01 = remap01(terrainHeight, shoreHeight, heightMinMax.y);
                float flatHeight01 = remap01(aboveShore01, 0, _MaxFlatHeight);

                float4 texNoise = TriplanarSample(_NoiseTex, sampler_NoiseTex, i.positionOS, N, _NoiseScale);

                float biomeWeight = saturate(Blend(0, 0.5, i.terrain.x + i.terrain.y * -0.15));
                float channelBlend = Blend(0, 1, i.terrain.w);
                float flatBlendA = lerp(texNoise.r, texNoise.b, channelBlend);
                float spotBlend = lerp(-0.7, 0.7, Blend(0, 2, i.terrain.w));

                float3 flatColA = lerp(_FlatColA.rgb, _FlatColA2.rgb, saturate(flatHeight01 + (flatBlendA-0.5) * spotBlend));
                float3 flatColB = lerp(_FlatColB.rgb, _FlatColB2.rgb, saturate(flatHeight01 + (flatBlendA-0.5) * spotBlend));
                float3 flatTerrainCol = lerp(flatColA, flatColB, biomeWeight);

                float shoreBlendWeight = 1 - Blend(_ShoreHeight, _ShoreBlend, flatHeight01);
                float3 shoreCol = _ShoreCol.rgb;
                flatTerrainCol = lerp(flatTerrainCol, shoreCol, shoreBlendWeight);

                float3 sphereTangent = normalize(float3(-sphereNormal.z, 0, sphereNormal.x));
                float3 normalTangent = normalize(N - sphereNormal * dot(N, sphereNormal));
                float banding = dot(sphereTangent, normalTangent) * 0.5 + 0.5;
                banding += (texNoise.a - 0.5) * 0.1;
                banding = (int)(banding * (_SteepBands + 1)) / _SteepBands;
                banding = (abs(banding - 0.5) * 2 - 0.5) * _SteepBandStrength;
                float3 steepTerrainCol = lerp(_SteepColA.rgb, _SteepColB.rgb, aboveShore01 + banding);

                float flatBlendNoise = (texNoise.b - 0.5) * -_FlatToSteepNoise;
                float flatStrength = 1 - Blend(_SteepnessThreshold + flatBlendNoise, saturate(_FlatToSteepBlend), steepness);

                float3 col = lerp(steepTerrainCol, flatTerrainCol, flatStrength);
                col = lerp(col, _FresnelCol.rgb, i.fresnel);

                SurfaceData surface; ZERO_INITIALIZE(SurfaceData, surface);
                surface.albedo = col;
                surface.normalTS = float3(0,0,1);
                surface.metallic = _Metallic;
                surface.smoothness = max(steepness * _Glossiness, _GlossinessFlat);
                surface.emission = 0;
                surface.occlusion = 1;
                surface.alpha = 1;

                InputData inputData;
                Celestial_InitializeInputData(i.positionWS, N, V, inputData);

                return UniversalFragmentPBR(inputData, surface);
            }
            ENDHLSL
        }
    }
}