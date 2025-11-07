Shader "Custom/ToonWater"
{
    Properties
    {
        _DepthGradientShallow("Shallow Color", Color) = (0.325, 0.807, 0.971, 0.725)
        _DepthGradientMid("Mid Depth Color", Color) = (0.086, 0.407, 1, 0.749)
        _DepthGradientDeep("Deep Color", Color) = (0.02, 0.15, 0.3, 0.8)

        _DepthMidDistance("Mid Depth Distance", Float) = 0.5
        _DepthMaxDistance("Max Depth Distance", Float) = 1

        _FoamColor("Foam Color", Color) = (1, 1, 1, 1)

        _SurfaceNoise("Surface Noise", 2D) = "white" {}
        _SurfaceNoiseScroll("Noise Scroll Speed", Vector) = (0.03, 0.03, 0, 0)
        _SurfaceNoiseCutoff("Noise Cutoff", Range(0, 1)) = 0.777

        _SurfaceDistortion("Surface Distortion", 2D) = "white" {}
        _SurfaceDistortionAmount("Distortion Amount", Range(0, 1)) = 0.27

        _FoamMaxDistance("Foam Max Distance", Float) = 0.4
        _FoamMinDistance("Foam Min Distance", Float) = 0.04
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #define SMOOTHSTEP_AA 0.01

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Alpha blend helper
            float4 alphaBlend(float4 top, float4 bottom)
            {
                float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
                float alpha = top.a + bottom.a * (1 - top.a);
                return float4(color, alpha);
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;    
                float2 noiseUV : TEXCOORD0;
                float2 distortUV : TEXCOORD1;
                float4 screenPosition : TEXCOORD2;
                float3 viewNormal : NORMAL;
            };

            sampler2D _SurfaceNoise;
            float4 _SurfaceNoise_ST;

            sampler2D _SurfaceDistortion;
            float4 _SurfaceDistortion_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPosition = ComputeScreenPos(o.vertex);
                o.distortUV = TRANSFORM_TEX(v.uv, _SurfaceDistortion);
                o.noiseUV = TRANSFORM_TEX(v.uv, _SurfaceNoise);
                o.viewNormal = COMPUTE_VIEW_NORMAL;
                return o;
            }

            float4 _DepthGradientShallow;
            float4 _DepthGradientMid;
            float4 _DepthGradientDeep;
            float4 _FoamColor;

            float _DepthMidDistance;
            float _DepthMaxDistance;
            float _FoamMaxDistance;
            float _FoamMinDistance;
            float _SurfaceNoiseCutoff;
            float _SurfaceDistortionAmount;
            float2 _SurfaceNoiseScroll;

            sampler2D _CameraDepthTexture;
            sampler2D _CameraNormalsTexture;

            float4 frag (v2f i) : SV_Target
            {
                // Depth behind water surface
                float existingDepth01 = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPosition)).r;
                float existingDepthLinear = LinearEyeDepth(existingDepth01);

                // Difference between water surface and underlying object
                float depthDifference = existingDepthLinear - i.screenPosition.w;

                if (depthDifference < 0)
                    return _DepthGradientShallow;

                // Depth-based water color
                float depth01 = saturate(depthDifference / _DepthMaxDistance);
                float midPoint = _DepthMidDistance / _DepthMaxDistance;

                float4 waterColor;
                if (depth01 < midPoint)
                {
                    float t = depth01 / midPoint;
                    waterColor = lerp(_DepthGradientShallow, _DepthGradientMid, t);
                }
                else
                {
                    float t = (depth01 - midPoint) / (1 - midPoint);
                    waterColor = lerp(_DepthGradientMid, _DepthGradientDeep, t);
                }

                // Foam calculation based on normals
                float3 existingNormal = tex2Dproj(_CameraNormalsTexture, UNITY_PROJ_COORD(i.screenPosition));
                float3 normalDot = saturate(dot(existingNormal, i.viewNormal));
                float foamDistance = lerp(_FoamMaxDistance, _FoamMinDistance, normalDot);
                float foamDepth01 = saturate(depthDifference / foamDistance);

                float surfaceNoiseCutoff = foamDepth01 * _SurfaceNoiseCutoff;

                // Distortion sampling
                float2 distortSample = (tex2D(_SurfaceDistortion, i.distortUV).xy * 2 - 1) * _SurfaceDistortionAmount;

                // Noise sampling with distortion and time
                float2 noiseUV = float2(
                    (i.noiseUV.x + _Time.y * _SurfaceNoiseScroll.x) + distortSample.x, 
                    (i.noiseUV.y + _Time.y * _SurfaceNoiseScroll.y) + distortSample.y
                );

                float surfaceNoiseSample = tex2D(_SurfaceNoise, noiseUV).r;
                float surfaceNoise = smoothstep(
                    surfaceNoiseCutoff - SMOOTHSTEP_AA, 
                    surfaceNoiseCutoff + SMOOTHSTEP_AA, 
                    surfaceNoiseSample
                );

                float4 surfaceNoiseColor = _FoamColor;
                surfaceNoiseColor.a *= surfaceNoise;

                // Blend foam with water
                return alphaBlend(surfaceNoiseColor, waterColor);
            }
            ENDCG
        }
    }
}
