Shader "UI/Indicator"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        [Toggle(FLARE)] _Flare("Flare", Int) = 0
        [Toggle(DEBUG)] _Debug("Debug", Int) = 0

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
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma shader_feature FLARE
            #pragma shader_feature DEBUG

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #include "Math.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct vertData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 data0 : TEXCOORD1;
                float2 data1 : TEXCOORD2;
                fixed4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct fragData
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                float4 worldPosition : TEXCOORD1;
                half4  mask : TEXCOORD2;
                float2 data0 : TEXCOORD3;
                float2 data1 : TEXCOORD4;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;

            fragData vert (vertData IN)
            {
                fragData OUT;
                
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                
                float2 pixelSize = OUT.vertex.w / float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                OUT.uv = rotate45(IN.uv, half2(0.5, 0.5));
                OUT.mask = half4(IN.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));
                
                OUT.color = IN.color * _Color;
                
                OUT.data0 = IN.data0;
                OUT.data1 = IN.data1;
                
                return OUT;
            }

            fixed4 frag (vertData IN) : SV_Target
            {
                float radius    = IN.data0.x;
                float thickness = IN.data0.y * 0.5;
                float smooth    = IN.data1.x;
                float size      = IN.data1.y;
                
                half2 pattern = IN.uv * size;
                half2 position = ceil(pattern) - size * 0.5 - 0.5;
                
                float value = ring(
                    position,
                    (radius + thickness) * size,
                    (radius - thickness) * size,
                    smooth * size
                );
                
                clip(value);
                
                fixed4 color = tex2D(_MainTex, pattern) + _TextureSampleAdd;
                
                #ifdef FLARE
                color.a *= lerp(0.2, 1, noise(position)) * value;
                color += color * 5 * smoothstep(0.95, 1, noise(position) * value);
                #else
                color.a *= lerp(0.5, 0.7, noise(position)) * value;
                #endif
                
                #ifdef DEBUG
                float debug = ring(
                    IN.uv - 0.5,
                    radius + thickness,
                    radius - thickness,
                    thickness
                );
                color += fixed4(0, 1, 0, 1) * step(0.99, debug);
                color += fixed4(1, 0, 0, 1) * (step(0.98, 1 - debug) - step(1, 1 - debug));
                #endif
                
                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
                #endif
                
                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif
                
                return color * IN.color;
            }
            ENDCG
        }
    }
}
