Shader "Celestial/MoonA"
{
    Properties
    {
        [Header(Colours)]
        _PrimaryColA ("Primary A", Color) = (1,1,1,1)
        _SecondaryColA ("Secondary A", Color) = (1,1,1,1)
        _PrimaryColB ("Primary B", Color) = (1,1,1,1)
        _SecondaryColB ("Secondary B", Color) = (1,1,1,1)
        _EjectaCol ("Ejecta Col", Color) = (1,1,1,1)

        _BiomeBlendStrength ("Biome Blend", Float) = 2
        _BiomeWarpStrength("Biome Warp", Float) = 0
        [Toggle()] _UseEjecta("Use Ejecta", float) = 1
        _EjectaBrightness("Ejecta Brightness", Float) = 0.1

        [Header(Normals)]
        [NoScaleOffset] _CraterRay ("Crater Ray", 2D) = "white" {}
        [NoScaleOffset] _MainTex ("Albedo (RGB)", 2D) = "white" {}
        [NoScaleOffset] _NormalMapFlat ("Normal Map Flat", 2D) = "bump" {}
        [NoScaleOffset] _NormalMapSteep ("Normal Map Steep", 2D) = "bump" {}
        _NormalMapFlatScale ("Normal Map Flat Scale", Float) = 10
        _NormalMapSteepScale ("Normal Map Steep Scale", Float) = 10
        _MainTexScale ("Main Tex Scale", Float) = 10

        _RandomBiomeValues ("Random Biome Values", Vector) = (0,0,0,0)
        _NormalMapStrength ("Normal Map Strength", Range(0,1)) = 0.3

        [Header(Other)]
        _FresnelCol("Fresnel Colour", Color) = (1,1,1,1)
        _FresnelStrengthNear("Fresnel Strength Min", float) = 2
        _FresnelStrengthFar("Fresnel Strength Max", float) = 5
        _FresnelPow("Fresnel Power", float) = 12

        _SmoothnessA ("Smoothness A", Range(0,1)) = 0.5
        _SmoothnessB ("Smoothness B", Range(0,1)) = 0.5
        _SmoothnessEjecta ("Smoothness Ejecta", Range(0,1)) = 0.5
        _MetallicA ("Metallic A", Range(0,1)) = 0.0
        _MetallicB ("Metallic B", Range(0,1)) = 0.0

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

            #include "../Includes/Math.cginc"

            TEXTURE2D(_MainTex);           SAMPLER(sampler_MainTex);
            TEXTURE2D(_CraterRay);         SAMPLER(sampler_CraterRay);
            TEXTURE2D(_NormalMapFlat);     SAMPLER(sampler_NormalMapFlat);
            TEXTURE2D(_NormalMapSteep);    SAMPLER(sampler_NormalMapSteep);

            CBUFFER_START(UnityPerMaterial)
            float4 _PrimaryColA,_SecondaryColA,_PrimaryColB,_SecondaryColB,_EjectaCol,_FresnelCol,_RandomBiomeValues,_TestParams;
            float _BiomeBlendStrength,_BiomeWarpStrength,_EjectaBrightness,_UseEjecta;
            float _NormalMapStrength,_NormalMapFlatScale,_NormalMapSteepScale,_MainTexScale;
            float _SmoothnessA,_SmoothnessB,_SmoothnessEjecta,_MetallicA,_MetallicB;
            float _FresnelStrengthNear,_FresnelStrengthFar,_FresnelPow;
            float2 heightMinMax;
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
                float4 craterUV   : TEXCOORD5;
                float  fresnel    : TEXCOORD6;
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

                float craterDst = v.texcoord.y;
                float2 craterUV = 0.5 + float2(cos(v.texcoord.x), sin(v.texcoord.x)) * craterDst;
                o.craterUV = float4(craterUV.xy, craterDst, 0);

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

            float3 UnpackNormalSimple(float4 p)
            {
                float3 n;
                n.xy = p.xy * 2 - 1;
                n.z = sqrt(saturate(1 - dot(n.xy, n.xy)));
                return n;
            }

            float3 TriplanarNormalTS(Texture2D tex, SamplerState samp, float3 position, float3 normalWS, float scale)
            {
                float2 uvX = position.zy * scale;
                float2 uvY = position.xz * scale;
                float2 uvZ = position.xy * scale;

                float3 nX = UnpackNormalSimple(SAMPLE_TEXTURE2D(tex, samp, uvX));
                float3 nY = UnpackNormalSimple(SAMPLE_TEXTURE2D(tex, samp, uvY));
                float3 nZ = UnpackNormalSimple(SAMPLE_TEXTURE2D(tex, samp, uvZ));

                float3 w = normalWS * normalWS;
                w /= dot(w, float3(1,1,1));
                return normalize(nX * w.x + nY * w.y + nZ * w.z);
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

                float height01 = remap01(length(i.positionOS), heightMinMax.x, heightMinMax.y);
                const float maxSteepnessApprox = 0.3;
                float steepness = 1 - dot(N, normalize(i.positionOS));
                steepness = remap01(steepness, 0, maxSteepnessApprox);

                float warpNoise = Blend(0, 3, i.terrain.z);
                float4 texNoise = TriplanarSample(_MainTex, sampler_MainTex, i.positionOS, N, _MainTexScale);

                float3 normalMapFlat = TriplanarNormalTS(_NormalMapFlat, sampler_NormalMapFlat, i.positionOS, N, _NormalMapFlatScale);
                float3 normalMapSteep = TriplanarNormalTS(_NormalMapSteep, sampler_NormalMapSteep, i.positionOS, N, _NormalMapSteepScale);
                float normalBlend = lerp(texNoise.r, texNoise.g, Blend(0, 2, i.terrain.z));
                float3 flatAndSteepNormal = normalize(lerp(normalMapFlat, normalMapSteep, normalBlend));
                float3 normalTS = normalize(lerp(flatAndSteepNormal, normalMapSteep, steepness));
                normalTS = normalize(lerp(float3(0,0,1), normalTS, _NormalMapStrength));

                float heightNoiseA = -texNoise.g * steepness - (texNoise.b - 0.5) * 0.7 + (texNoise.a - 0.5) * _RandomBiomeValues.x;
                float heightNoiseB = (texNoise.g-0.5) * _RandomBiomeValues.y + (texNoise.r-0.5) * _RandomBiomeValues.z;
                float heightBlendWeightA = Blend(0.5, 0.6, height01 + heightNoiseA) * warpNoise;
                float heightBlendWeightB = Blend(0.5, 0.6, height01 + heightNoiseB) * warpNoise;
                float3 colBiomeA = lerp(_PrimaryColA.rgb, _SecondaryColA.rgb, heightBlendWeightA);
                float3 colBiomeB = lerp(_PrimaryColB.rgb, _SecondaryColB.rgb, heightBlendWeightB);

                float biomeNoise = dot(texNoise.ga - 0.5, _RandomBiomeValues.zw) * 4;
                float biomeWeight = Blend(0.8 + i.terrain.z * _BiomeWarpStrength, _BiomeBlendStrength + warpNoise * 15, i.terrain.w + biomeNoise);
                float3 biomeCol = lerp(colBiomeA, colBiomeB, biomeWeight);

                float ejecta = SAMPLE_TEXTURE2D(_CraterRay, sampler_CraterRay, i.craterUV.xy).r;
                float craterAlpha = i.craterUV.z < 1;
                float3 compositeCol = lerp(biomeCol, _EjectaCol.rgb, saturate(ejecta * 1.5) * craterAlpha * _UseEjecta);
                compositeCol = lerp(compositeCol, _FresnelCol.rgb, i.fresnel);

                SurfaceData surface; ZERO_INITIALIZE(SurfaceData, surface);
                surface.albedo = compositeCol;
                surface.normalTS = normalTS;
                surface.metallic = lerp(_MetallicA, _MetallicB, biomeWeight);
                float smoothness = lerp(_SmoothnessA, _SmoothnessB, biomeWeight);
                smoothness = lerp(smoothness, _SmoothnessEjecta, saturate(ejecta * 1.5) * craterAlpha);
                surface.smoothness = smoothness;
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