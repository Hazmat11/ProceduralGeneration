Shader "Custom/TerrainShader"
{
    Properties
    {

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        #pragma target 3.0

        sampler2D terrainGradient;
        float minHeight;
        float maxHeight;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 worldPosY = IN.worldPos.y;
            float heightValue = saturate((worldPosY - minHeight) / (maxHeight - minHeight));

            o.Albedo = tex2D(terrainGradient, float2(0,heightValue));
        }
        ENDCG
    }
    FallBack "Diffuse"
}