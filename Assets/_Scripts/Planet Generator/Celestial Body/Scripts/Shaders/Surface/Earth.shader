Shader "Celestial/Earth"
{
    Properties
    {
        [Header(Flat Terrain)]
        _ShoreLow("Shore Low", Color) = (0,0,0,1)
        _ShoreHigh("Shore High", Color) = (0,0,0,1)
        _FlatLowA("Flat Low A", Color) = (0,0,0,1)
        _FlatHighA("Flat High A", Color) = (0,0,0,1)
        _FlatLowB("Flat Low B", Color) = (0,0,0,1)
        _FlatHighB("Flat High B", Color) = (0,0,0,1)

        _FlatColBlend("Colour Blend", Range(0,3)) = 1.5
        _FlatColBlendNoise("Blend Noise", Range(0,1)) = 0.3
        _ShoreHeight("Shore Height", Range(0,0.2)) = 0.05
        _ShoreBlend("Shore Blend", Range(0,0.2)) = 0.03
        _MaxFlatHeight("Max Flat Height", Range(0,1)) = 0.5

        [Header(Steep Terrain)]
        _SteepLow("Steep Colour Low", Color) = (0,0,0,1)
        _SteepHigh("Steep Colour High", Color) = (0,0,0,1)
        _SteepBands("Steep Bands", Range(1,20)) = 8
        _SteepBandStrength("Band Strength", Range(-1,1)) = 0.5

        [Header(Flat to Steep Transition)]
        _SteepnessThreshold("Steep Threshold", Range(0,1)) = 0.5
        _FlatToSteepBlend("Flat to Steep Blend", Range(0,0.3)) = 0.1
        _FlatToSteepNoise("Flat to Steep Noise", Range(0,0.2)) = 0.1

        [Header(Snowy Poles)]
        [Toggle] _UseSnowyPoles("Use Poles", Float) = 0
        _SnowCol("Snow Colour", Color) = (1,1,1,1)
        _SnowLongitude("Snow Longitude", Range(0,1)) = 0.8
        _SnowBlend("Snow Blend", Range(0,0.2)) = 0.1
        _SnowSpecular("Snow Specular", Range(0,1)) = 1
        _SnowHighlight("Snow Highlight", Range(1,2)) = 1.2
        _SnowNoiseA("Snow Noise A", Range(0,10)) = 5
        _SnowNoiseB("Snow Noise B", Range(0,10)) = 4

        [Header(Noise)]
        [NoScaleOffset]_NoiseTex("Noise Texture", 2D) = "white" {}
        _NoiseScale("Noise Scale", Float) = 1
        _NoiseScale2("Noise Scale2", Float) = 1

        [Header(Other)]
        _FresnelCol("Fresnel Colour", Color) = (1,1,1,1)
        _FresnelStrengthNear("Fresnel Strength Min", Float) = 2
        _FresnelStrengthFar("Fresnel Strength Max", Float) = 5
        _FresnelPow("Fresnel Power", Float) = 2
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0
        //_ShadowBrightness("Shadow Brightness", Range(0, 1)) = 0.15

        _TestParams("Test Params", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT

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
            float4 _ShoreLow,_ShoreHigh,_FlatLowA,_FlatHighA,_FlatLowB,_FlatHighB;
            float4 _SteepLow,_SteepHigh,_SnowCol,_FresnelCol,_TestParams;

            float _FlatColBlend,_FlatColBlendNoise,_ShoreHeight,_ShoreBlend,_MaxFlatHeight;
            float _SteepBands,_SteepBandStrength;
            float _SteepnessThreshold,_FlatToSteepBlend,_FlatToSteepNoise;

            float _UseSnowyPoles,_SnowLongitude,_SnowBlend,_SnowSpecular,_SnowHighlight,_SnowNoiseA,_SnowNoiseB;
            float _NoiseScale,_NoiseScale2;
            float _Glossiness,_Metallic;
            float _FresnelStrengthNear,_FresnelStrengthFar,_FresnelPow;
            //float _ShadowBrightness;

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
                float4 terrain    : TEXCOORD3;
                float  fresnel    : TEXCOORD4;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionOS = v.positionOS.xyz;
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
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

                float3 blendWeight = normal * normal;
                blendWeight /= dot(blendWeight, float3(1,1,1));

                return colX * blendWeight.x + colY * blendWeight.y + colZ * blendWeight.z;
            }

            float3 ViewDirWS(float3 posWS) { return normalize(GetWorldSpaceViewDir(posWS)); }

            void Celestial_InitializeInputData(float3 pWS, float3 nWS, float3 vWS, out InputData input)
            {
                ZERO_INITIALIZE(InputData, input);
                input.positionWS = pWS;
                input.normalWS = normalize(nWS);
                input.viewDirectionWS = normalize(vWS);
                input.shadowCoord = TransformWorldToShadowCoord(pWS);
                input.fogCoord = 0;
                input.bakedGI = SampleSH(input.normalWS);
            }

            float4 frag (Varyings i) : SV_Target
            {
                float3 N = normalize(i.normalWS);
                float3 V = ViewDirWS(i.positionWS);

                // ===== Steepness =====
                float3 sphereNormal = normalize(i.positionOS);
                float steepness = 1 - dot(sphereNormal, N);
                steepness = remap01(steepness, 0, 0.65);

                // ===== Height =====
                float terrainHeight = length(i.positionOS);
                float shoreHeight = lerp(heightMinMax.x, 1, oceanLevel);
                float aboveShore01 = remap01(terrainHeight, shoreHeight, heightMinMax.y);
                float flatHeight01 = remap01(aboveShore01, 0, _MaxFlatHeight);

                // ===== Noise =====
                float4 noise1 = TriplanarSample(_NoiseTex, sampler_NoiseTex, i.positionOS, N, _NoiseScale);
                float4 noise2 = TriplanarSample(_NoiseTex, sampler_NoiseTex, i.positionOS, N, _NoiseScale2);

                // ===== Flat terrain =====
                float flatColBlend = Blend(0,_FlatColBlend,(flatHeight01-0.5)+(noise1.b-0.5)*_FlatColBlendNoise);

                float3 flatA = lerp(_FlatLowA.rgb,_FlatHighA.rgb,flatColBlend);
                flatA = lerp(flatA,(_FlatLowA.rgb+_FlatHighA.rgb)*0.5,noise1.a);

                float3 flatB = lerp(_FlatLowB.rgb,_FlatHighB.rgb,flatColBlend);
                flatB = lerp(flatB,(_FlatLowB.rgb+_FlatHighB.rgb)*0.5,noise1.a);

                float biome = Blend(_TestParams.x,_TestParams.y,i.terrain.x);
                biome = Blend(0,_TestParams.z, i.positionOS.x + i.terrain.x * _TestParams.x + i.terrain.y * _TestParams.y);

                float3 flatCol = lerp(flatA,flatB,biome);

                // ===== Shore =====
                float shoreBlendW = 1 - Blend(_ShoreHeight,_ShoreBlend,flatHeight01);
                float3 shoreCol = lerp(_ShoreLow.rgb,_ShoreHigh.rgb,remap01(aboveShore01,0,_ShoreHeight));
                shoreCol = lerp(shoreCol,(_ShoreLow.rgb+_ShoreHigh.rgb)*0.5,noise1.g);

                flatCol = lerp(flatCol,shoreCol,shoreBlendW);

                // ===== Steep terrain =====
                float3 sphereTangent = normalize(float3(-sphereNormal.z,0,sphereNormal.x));
                float3 normalTangent = normalize(N - sphereNormal * dot(N,sphereNormal));

                float banding = dot(sphereTangent,normalTangent)*0.5+0.5;
                banding = floor(banding*(_SteepBands+1))/_SteepBands;
                banding = (abs(banding-0.5)*2-0.5)*_SteepBandStrength;

                float3 steepCol = lerp(_SteepLow.rgb,_SteepHigh.rgb,aboveShore01+banding);

                // ===== Flat to steep =====
                float flatNoise = (noise2.r-0.5)*_FlatToSteepNoise;
                float flatStrength = 1-Blend(_SteepnessThreshold+flatNoise,_FlatToSteepBlend,steepness);
                float flatFalloff = 1-Blend(_MaxFlatHeight+flatNoise,_FlatToSteepBlend,aboveShore01);
                flatStrength *= flatFalloff;

                // ===== Snow =====
                float snowLineNoise = i.terrain.y*_SnowNoiseA*0.01 + (noise1.b-0.5)*_SnowNoiseB*0.01;
                float snowWeight = Blend(_SnowLongitude,_SnowBlend,abs(i.positionOS.y+snowLineNoise)) * _UseSnowyPoles;

                float snowSpeckle = 1 - noise2.g * 0.05;
                float3 snowCol = _SnowCol.rgb * lerp(1,_SnowHighlight,aboveShore01+banding) * snowSpeckle;

                // ===== Composite =====
                float3 col = lerp(steepCol,flatCol,flatStrength);
                col = lerp(col,snowCol,snowWeight);
                col = lerp(col,_FresnelCol.rgb,i.fresnel);

                // ===== PBR =====
                SurfaceData surface; 
                ZERO_INITIALIZE(SurfaceData, surface);
                surface.albedo = col;
                surface.normalTS = float3(0,0,1);

                #if defined(_SPECULAR_SETUP)
                    surface.specular = float3(0.04, 0.04, 0.04);
                #else
                    surface.metallic = _Metallic;
                #endif

                float avg = (col.r + col.g + col.b) * (1.0/3.0);
                surface.smoothness = max(avg * _Glossiness, snowWeight * _SnowSpecular);
                surface.emission = 0;
                surface.occlusion = 1;
                surface.alpha = 1;

                #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
                    surface.clearCoatMask = 0;
                    surface.clearCoatSmoothness = 0;
                #endif

                InputData inputData;
                Celestial_InitializeInputData(i.positionWS, N, V, inputData);

                return UniversalFragmentPBR(inputData, surface);

                //return float4(col, 1);
            }
            ENDHLSL
        }
    }
}