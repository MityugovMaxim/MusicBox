Shader "UI/Circle"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Blend SrcAlpha One
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct vertData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 data0 : TEXCOORD1;
                float2 data1 : TEXCOORD2;
                fixed4 color : COLOR;
            };

            struct fragData
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 data0 : TEXCOORD1;
                float2 data1 : TEXCOORD2;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            fixed4 _Color;

            float circle(half2 _Position, float _Radius, float _Smooth)
            {
                float value = length(_Position);
                return smoothstep(_Radius, _Radius - _Smooth, value);
            }

            float ring(half2 _Position, float _OutRadius, float _InRadius, float _Smooth)
            {
                float value     = length(_Position);
                float outCircle = smoothstep(_OutRadius, _OutRadius - _Smooth, value);
                float inCircle  = smoothstep(_InRadius, _InRadius + _Smooth, value);
                return outCircle * inCircle;
            }

            float noise(half2 _Position)
            {
                float random = 2920 * sin(_Position.x * 21942 + _Position.y * 171324 + 8912) + _Time.y;
                return sin(random) * 0.5 + 0.5;
            }

            float remap(float value, float l1, float h1, float l2, float h2)
            {
                return l2 + (value - l1) * (h2 - l2) / (h1 - l1);
            }

            fragData vert (vertData IN)
            {
                fragData OUT;
                
                const half2 offset = half2(0.5, 0.5);
                const float rad = 0.70710678118;
                const float2x2 rotation = float2x2(
                    rad, -rad,
                    rad, rad
                );
                
                half2 uv = IN.uv;
                uv -= offset;
                uv = mul(uv, rotation);
                uv += offset;
                
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = uv;
                OUT.data0 = IN.data0;
                OUT.data1 = IN.data1;
                OUT.color = _Color * IN.color;
                return OUT;
            }

            fixed4 frag (fragData IN) : SV_Target
            {
                float radius    = IN.data0.x;
                float thickness = IN.data0.y;
                float smooth    = IN.data1.x;
                float size      = IN.data1.y;
                
                half2 pattern = IN.uv * size;
                half2 position = ceil(pattern) - size * 0.5 - 0.5;
                
                float r = ring(position, radius * size, (radius - thickness) * size, smooth * size);
                
                fixed4 color = tex2D(_MainTex, pattern);
                
                color.a *= lerp(0.01, 1, r) * lerp(0, 1, noise(position));
                
                color += (color + color + color) * max(0, remap(noise(position) * color.a, 0.97, 1, 0, 1));
                
                return color * IN.color;
            }
            ENDCG
        }
    }
}
