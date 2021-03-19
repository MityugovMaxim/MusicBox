Shader "UI/Circle"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha One
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

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

            float rand(float2 n)
            { 
                return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453 + _Time.x);
            }

            float noise(float2 p)
            {
                float2 ip = floor(p);
                float2 u = frac(p);
                u = u * u * (3.0 - 2.0 * u);
                
                float res = lerp(
                    lerp(rand(ip), rand(ip + float2(1.0,0.0)), u.x),
                    lerp(rand(ip + float2(0.0,1.0)), rand(ip + float2(1.0,1.0)), u.x), u.y);
                return res * res;
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
                
                float value = ring(position, radius * size, (radius - thickness) * size, smooth * size);
                
                fixed4 color = tex2D(_MainTex, pattern);
                
                color.a *= lerp(0.02, 1, value) * lerp(0, 1, noise(position));
                
                float grayscale = 0.21 * color.r + 0.71 * color.g + 0.07 * color.b;
                
                color = lerp(fixed4(grayscale, grayscale, grayscale, color.a), color, value);
                
                color += color * max(0, remap(noise(position) * value, 0.5, 1, 0, 1));
                
                return color * IN.color;
            }
            ENDCG
        }
    }
}
