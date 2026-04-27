Shader "LRF/HitDot"
{
    Properties
    {
        _DotColor    ("Dot Color",    Color)         = (1, 0.1, 0.1, 1)
        _DotRadius   ("Dot Radius",   Range(0, 0.5)) = 0.15
        _GlowFalloff ("Glow Falloff", Range(1, 8))   = 4.0
        _Intensity   ("Intensity",    Float)          = 3.0
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

            float4 _DotColor;
            float  _DotRadius, _GlowFalloff, _Intensity;

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
                float2 center = float2(0.5, 0.5);
                float  dist   = length(i.uv - center);

                float core = 1.0 - smoothstep(_DotRadius * 0.3, _DotRadius, dist);
                float glow = pow(1.0 - smoothstep(0.0, 0.5, dist), _GlowFalloff);

                float brightness = (core + glow * 0.4) * _Intensity;

                return _DotColor * brightness;
            }
            ENDCG
        }
    }
}
