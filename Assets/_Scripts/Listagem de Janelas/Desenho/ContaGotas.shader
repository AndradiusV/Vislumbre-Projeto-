Shader "Unlit/ContaGotas"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            float4 _PixelCoord;
            float4 _SourceSize;

            fixed4 frag(v2f_img i) : SV_Target
            {
                // Centro do pixel desejado
                float2 uv = (_PixelCoord.xy + 0.5) / _SourceSize.xy;

                // Corrige a orientação quando necessário
                #if UNITY_UV_STARTS_AT_TOP
                if (_MainTex_TexelSize.y < 0)
                    uv.y = 1.0 - uv.y;
                #endif

                return tex2Dlod(_MainTex, float4(uv, 0, 0));
            }

            ENDCG
        }
    }
}
