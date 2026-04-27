Shader "LRF/SevenSegDisplay"
{
    Properties
    {
        _Distance  ("Distance", Float) = 0.0
        _Hit       ("Hit", Float) = 1.0
        _OnColor   ("On Color",  Color) = (1, 0.2, 0.2, 1)
        _OffColor  ("Off Color", Color) = (0.05, 0.05, 0.05, 1)
        _BGColor   ("Background",Color) = (0, 0, 0, 1)
        _SegWidth  ("Segment Width", Float) = 0.10
        _SegGap    ("Segment Gap",   Float) = 0.03
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Distance, _Hit;
            float4 _OnColor, _OffColor, _BGColor;
            float _SegWidth, _SegGap;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f     { float4 pos    : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            // Segment bit definitions [bit6=g, bit5=f, bit4=e, bit3=d, bit2=c, bit1=b, bit0=a]
            //   a
            //  f b
            //   g
            //  e c
            //   d
            static const int SEG[11] = {
                0x3F, // 0: abcdef
                0x06, // 1: bc
                0x5B, // 2: abdeg
                0x4F, // 3: abcdg
                0x66, // 4: bcfg
                0x6D, // 5: acdfg
                0x7D, // 6: acdefg
                0x07, // 7: abc
                0x7F, // 8: all
                0x6F, // 9: abcdfg
                0x00  // 10: blank
            };

            float segHit(float2 uv, int seg, float sw, float gap)
            {
                float g = gap, s = sw;
                float x = uv.x, y = uv.y;
                float h = 0.0;

                if      (seg == 0) h = step(g,x)*step(x,1-g)*step(1-s-g,y)*step(y,1-g);
                else if (seg == 1) h = step(0.5+g,y)*step(y,1-g)*step(1-s-g,x)*step(x,1-g);
                else if (seg == 2) h = step(g,y)*step(y,0.5-g)*step(1-s-g,x)*step(x,1-g);
                else if (seg == 3) h = step(g,x)*step(x,1-g)*step(g,y)*step(y,s+g);
                else if (seg == 4) h = step(g,y)*step(y,0.5-g)*step(g,x)*step(x,s+g);
                else if (seg == 5) h = step(0.5+g,y)*step(y,1-g)*step(g,x)*step(x,s+g);
                else if (seg == 6) h = step(g,x)*step(x,1-g)*step(0.5-s*0.5,y)*step(y,0.5+s*0.5);

                return h;
            }

            float drawDigit(float2 uv, int digit, float sw, float gap)
            {
                int bits = SEG[clamp(digit, 0, 10)];
                float result = 0.0;
                for (int i = 0; i < 7; i++)
                    result = max(result, segHit(uv, i, sw, gap) * (float)((bits >> i) & 1));
                return result;
            }

            float drawPlus(float2 uv, float sw, float gap)
            {
                float g = gap, s = sw;
                float x = uv.x, y = uv.y;
                float horiz = step(g,x)*step(x,1-g)*step(0.5-s*0.5,y)*step(y,0.5+s*0.5);
                float vert  = step(0.5-s*0.5,x)*step(x,0.5+s*0.5)*step(g,y)*step(y,1-g);
                return max(horiz, vert);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // 4 slots: [0]=prefix(+/blank), [1]=hundreds, [2]=tens, [3]=ones
                float slotW   = 0.25;
                int   slotIdx = (int)(uv.x / slotW);
                float2 localUV = float2(frac(uv.x / slotW), uv.y);

                bool overRange = (_Hit < 0.5);

                int dist = overRange ? 999 : (int)clamp(_Distance, 0, 999);

                int d2 = (dist / 100) % 10;
                int d1 = (dist /  10) % 10;
                int d0 =  dist        % 10;

                // Leading zero suppression (only when not over range)
                if (!overRange)
                {
                    if (d2 == 0) { d2 = 10;
                        if (d1 == 0) d1 = 10;
                    }
                }

                float lit = 0.0;

                if      (slotIdx == 0) { if (overRange) lit = drawPlus(localUV, _SegWidth, _SegGap); }
                else if (slotIdx == 1) lit = drawDigit(localUV, d2, _SegWidth, _SegGap);
                else if (slotIdx == 2) lit = drawDigit(localUV, d1, _SegWidth, _SegGap);
                else if (slotIdx == 3) lit = drawDigit(localUV, d0, _SegWidth, _SegGap);

                return lit > 0.0 ? _OnColor : lerp(_BGColor, _OffColor, 0.5);
            }
            ENDCG
        }
    }
}
