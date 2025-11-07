Shader "Custom/PlanetVertexColor_Diffuse"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _ColorBoost ("Vertex Color Boost", Range(0.2,2)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        float _ColorBoost;

        struct Input
        {
            float2 uv_MainTex;
            float4 color : COLOR;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
            fixed4 col = tex * IN.color * _ColorBoost;

            o.Albedo = col.rgb;
            o.Alpha  = col.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
