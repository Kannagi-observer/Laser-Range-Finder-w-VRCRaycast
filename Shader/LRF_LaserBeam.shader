Shader "LRF/LaserBeam"
{
    Properties
    {
        _BeamColor    ("Beam Color",     Color)          = (1, 0.1, 0.1, 1)
        _CoreWidth    ("Core Width",     Range(0, 0.5))  = 0.05
        _GlowWidth    ("Glow Width",     Range(0, 0.5))  = 0.25
        _GlowFalloff  ("Glow Falloff",   Range(1, 8))    = 3.0
        _Intensity    ("Intensity",      Float)           = 2.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend One One   // Additive
        ZWrite Off
        Cull Off        // 両面描画

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _BeamColor;
            float  _CoreWidth, _GlowWidth, _GlowFalloff, _Intensity;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f     { float4 pos    : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // UV.x: 横方向 (0=左端, 0.5=中心, 1=右端)
                // UV.y: 縦方向 (ビーム長方向、端フェードに使用)
                float cx = abs(i.uv.x - 0.5); // 中心からの距離 0~0.5

                // コア: 細く強い
                float core = 1.0 - smoothstep(_CoreWidth * 0.5, _CoreWidth, cx);

                // グロー: 外側に向かって減衰
                float glow = pow(1.0 - smoothstep(0.0, _GlowWidth, cx), _GlowFalloff);

                // 端フェード (UV.y の両端)
                float endFade = smoothstep(0.0, 0.05, i.uv.y)
                              * smoothstep(1.0, 0.95, i.uv.y);

                float brightness = (core + glow * 0.5) * endFade * _Intensity;

                return _BeamColor * brightness;
            }
            ENDCG
        }
    }
}
