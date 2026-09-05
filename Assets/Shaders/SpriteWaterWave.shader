Shader "Custom/SpriteWaterWave"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Frequency ("Wave Frequency", Float) = 15.0
        _Amplitude ("Wave Amplitude", Float) = 0.05
        _Speed ("Wave Speed", Float) = 4.0
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float _Frequency;
            float _Amplitude;
            float _Speed;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // En Unity, UV.y = 1.0 es la parte SUPERIOR del sprite.
                // Multiplicamos por IN.texcoord.y para que abajo (y=0) no se mueva 
                // y arriba (y=1) tenga el movimiento máximo.
                float wave = sin((IN.texcoord.x * _Frequency) + (_Time.y * _Speed)) * _Amplitude * IN.texcoord.y;
                
                // Aplicamos la onda al eje Y
                float2 distorted_uv = float2(IN.texcoord.x, IN.texcoord.y + wave);

                // Si la onda empuja el pixel fuera del borde superior (UV > 1.0), lo hacemos invisible
                if (distorted_uv.y > 1.0 || distorted_uv.y < 0.0)
                {
                    return fixed4(0,0,0,0);
                }

                // Muestreamos la textura del Sprite
                fixed4 c = tex2D(_MainTex, distorted_uv) * IN.color;
                c.rgb *= c.a; // Soporte para Sprite Blend (Premultiplied Alpha)
                return c;
            }
        ENDCG
        }
    }
}
