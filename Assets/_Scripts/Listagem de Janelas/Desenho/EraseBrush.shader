Shader "Custom/EraseBrush"
{
    Properties
    {
        _BrushSize ("Brush Size", Float) = 20
        _BrushPosition ("Brush Position", Vector) = (0, 0, 0, 0)
        _Resolution ("Resolution", Vector) = (640, 480, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Blend One Zero
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _BrushSize;
            float4 _BrushPosition;
            float4 _Resolution;

            v2f vert(appdata v)
            {
                v2f o;

                // Quad em coordenadas locais:
                // (-0.5,-0.5) até (0.5,0.5)
                float2 pixelPosition = _BrushPosition.xy + v.vertex.xy * _BrushSize;

                // Converte pixels para NDC [-1,1]
                float2 ndc;

                ndc.x = (pixelPosition.x / _Resolution.x) * 2.0 - 1.0;
                ndc.y = -((pixelPosition.y / _Resolution.y) * 2.0 - 1.0);

                o.vertex = float4(ndc, 0.0, 1.0);

                o.uv = v.uv;

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Centro do brush = (0.5, 0.5)
                float2 delta = i.uv - 0.5;

                // Distância ao centro
                float distanceSquared = dot(delta, delta);

                // Fora do círculo
                if (distanceSquared > 0.25)
                    discard;

                return float4(0,0,0,0);
            }

            ENDHLSL
        }
    }
}
