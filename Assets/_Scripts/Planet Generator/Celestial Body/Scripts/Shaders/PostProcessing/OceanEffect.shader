Shader "Hidden/Ocean"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_OceanAmbient ("Ocean Ambient", Range(0,1)) = 0.15
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "../Includes/Math.cginc"
			#include "../Includes/Triplanar.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 viewVector : TEXCOORD1;
			};

			v2f vert (appdata v) {
				v2f output;
				output.pos = UnityObjectToClipPos(v.vertex);
				output.uv = v.uv;
				float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
				output.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
				return output;
			}

			float4 colA;
			float4 colB;
			float4 specularCol;
			float depthMultiplier;
			float alphaMultiplier;
			float smoothness;
			float _OceanAmbient;

			sampler2D waveNormalA;
			sampler2D waveNormalB;
			float waveStrength;
			float waveNormalScale;
			float waveSpeed;

			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			float4 params;

			float planetScale;
			float3 oceanCentre;
			float oceanRadius;
			float3 dirToSun;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 originalCol = tex2D(_MainTex, i.uv);

				float3 rayPos = _WorldSpaceCameraPos;
				float viewLength = length(i.viewVector);
				float3 rayDir = i.viewVector / viewLength;

				float nonlin_depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				float sceneDepth = LinearEyeDepth(nonlin_depth) * viewLength;

				float2 hitInfo = raySphere(oceanCentre, oceanRadius, rayPos, rayDir);
				float dstToOcean = hitInfo.x;
				float dstThroughOcean = hitInfo.y;
				float3 rayOceanIntersectPos = rayPos + rayDir * dstToOcean - oceanCentre;

				float oceanViewDepth = min(dstThroughOcean, sceneDepth - dstToOcean);

				if (oceanViewDepth > 0) {
					float3 clipPlanePos = rayPos + i.viewVector * _ProjectionParams.y;

					float dstAboveWater = length(clipPlanePos - oceanCentre) - oceanRadius;

					float t = 1 - exp(-oceanViewDepth / planetScale * depthMultiplier);
					float alpha =  1-exp(-oceanViewDepth / planetScale * alphaMultiplier);
					float4 oceanCol = lerp(colA, colB, t);

					float3 oceanSphereNormal = normalize(rayOceanIntersectPos);

					float2 waveOffsetA = float2(_Time.x * waveSpeed, _Time.x * waveSpeed * 0.8);
					float2 waveOffsetB = float2(_Time.x * waveSpeed * - 0.8, _Time.x * waveSpeed * -0.3);
					float3 waveNormal = triplanarNormal(rayOceanIntersectPos, oceanSphereNormal, waveNormalScale / planetScale, waveOffsetA, waveNormalA);
					waveNormal = triplanarNormal(rayOceanIntersectPos, waveNormal, waveNormalScale / planetScale, waveOffsetB, waveNormalB);
					waveNormal = normalize(lerp(oceanSphereNormal, waveNormal, waveStrength));

					// normalize dirToSun (bezpieczeństwo) i zastosuj ambient/half-lambert-like fallback
					float3 L = normalize(dirToSun);
					float lambert = saturate(dot(oceanSphereNormal, L));

					// ambient fallback: minimalna jasność zdefiniowana przez _OceanAmbient
					float diffuseLighting = max(_OceanAmbient, lambert);

					// opcjonalnie: half-lambert zamiennie:
					// float diffuseLighting = lambert * 0.5 + 0.5;
					// diffuseLighting = max(diffuseLighting, _OceanAmbient);

					float specularAngle = acos(dot(normalize(L - rayDir), waveNormal));
					float specularExponent = specularAngle / (1 - smoothness);
					float specularHighlight = exp(-specularExponent * specularExponent);

					oceanCol *= diffuseLighting;
					oceanCol += specularHighlight * (dstAboveWater > 0) * specularCol;

					return originalCol * (1-alpha) + oceanCol * alpha;
				}

				return originalCol;
			}
			ENDCG
		}
	}
}