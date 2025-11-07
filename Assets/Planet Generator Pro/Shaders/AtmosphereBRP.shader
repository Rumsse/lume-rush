Shader "Custom/AtmosphereBRP_VisibleInDark_Fix"
{
    Properties
    {
        _Color ("Atmosphere Color", Color) = (0.4,0.7,1,0.5)
        _Intensity ("Intensity", Range(0,5)) = 1.0
        _PlanetPos ("Planet Position", Vector) = (0,0,0,0)
        _PlanetRadius ("Planet Radius", Float) = 5000.0
        _AtmoHeight ("Atmosphere Height", Float) = 300.0

        _VisibilityThreshold ("Visibility Threshold", Range(0,1)) = 0.05
        _VisibilitySmooth ("Visibility Smooth (blend range)", Range(0.001,0.5)) = 0.08
        _DirectLightBoost ("Direct Light Boost", Range(0,5)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Cull Front
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _Intensity;
            float4 _PlanetPos;
            float _PlanetRadius;
            float _AtmoHeight;

            float _VisibilityThreshold;
            float _VisibilitySmooth;
            float _DirectLightBoost;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 planetPos = _PlanetPos.xyz;
                float3 world = i.worldPos;

                float dist = distance(world, planetPos);
                float height = dist - _PlanetRadius;

                float t = saturate(height / _AtmoHeight);

                // Base density scaled by intensity
                float baseDensity = (1.0 - t) * _Intensity;

                // Horizon factor makes atmosphere denser at the horizon
                float3 viewDir = normalize(_WorldSpaceCameraPos - world);
                float3 normal = normalize(world - planetPos);
                float horizonFactor = 1.0 - saturate(dot(viewDir, normal) * 0.5 + 0.5);
                horizonFactor = pow(horizonFactor, 1.5);

                float alpha = baseDensity * (0.2 + 0.8 * horizonFactor);

                float finalAlpha = saturate(alpha * _Color.a);

                // Lighting (ambient + simplified direct light)
                float3 ambientCol = UNITY_LIGHTMODEL_AMBIENT.xyz;
                const float3 lumW = float3(0.2126, 0.7152, 0.0722);
                float ambientLum = dot(ambientCol, lumW);

                float3 lightVec = normalize(-_WorldSpaceLightPos0.xyz);
                float NdotL = max(0.0, dot(normal, lightVec));

                float direct = NdotL * _DirectLightBoost;

                float lightFactor = saturate(ambientLum + direct);

                // Visibility factor: visible in dark, fades in light
                float vis = 1.0 - smoothstep(_VisibilityThreshold, _VisibilityThreshold + _VisibilitySmooth, lightFactor);

                finalAlpha *= vis;

                fixed3 col = _Color.rgb;
                return fixed4(col, finalAlpha);
            }
            ENDCG
        }
    }
    FallBack Off
}
