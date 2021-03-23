Shader "UI/Indicator"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Grayscale ("Grayscale", Range(0, 1)) = 0
        [Toggle(FLARE)] _Flare("Flare", Int) = 0

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
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
            #pragma shader_feature FLARE

            #include "UnityCG.cginc"
            #include "Math.cginc"

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
            fixed4    _Color;
            float     _Grayscale;

            fragData vert (vertData IN)
            {
                fragData OUT;
                
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = rotate(IN.uv, half2(0.5, 0.5), 45);
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
                
                clip(value);
                
                fixed4 color = tex2D(_MainTex, pattern);
                
                #ifdef FLARE
                color.a *= lerp(0.2, 1, noise(position)) * value;
                color += color * 5 * smoothstep(0.95, 1, noise(position) * value);
                #else
                color.a *= lerp(0.5, 0.7, noise(position)) * value;
                #endif
                
                float grayscale = color.r * 0.21 + color.g * 0.72 + color.b * 0.07;
                
                color.rgb = lerp(color.rgb, float3(grayscale, grayscale, grayscale), _Grayscale);
                
                return color * IN.color;
            }
            ENDCG
        }
    }
}
