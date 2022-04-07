Shader "UI/OutlineGlow Indicator"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_MaskTex ("Mask Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_Speed ("Speed", Float) = 1
		
		_Ratio0 ("Ratio 0", Range(0, 1)) = 0.333334
		_Ratio1 ("Ratio 1", Range(0, 1)) = 0.666667
		_Frequency0 ("Frequency 0", Float) = 11
		_Frequency1 ("Frequency 1", Float) = 2
		
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		[Toggle(BACKGROUND_SCHEME)] _UseBackgroundScheme ("Use Background Scheme", Float) = 0
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
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
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

			#include "UIMask.cginc"
			#include "UnityCG.cginc"
			#include "Math.cginc"
			#include "Color.cginc"

			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP
			#pragma multi_compile_local _ BACKGROUND_SCHEME

			struct vertData
			{
				float4 vertex : POSITION;
				float4 color  : COLOR;
				float2 uv     : TEXCOORD0;
				float2 progress : TEXCOORD1;
				float2 size : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct fragData
			{
				float4 vertex : SV_POSITION;
				fixed4 color  : COLOR;
				half2 uv     : TEXCOORD0;
				half4  mask   : TEXCOORD1;
				half4 data : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			sampler2D _MaskTex;
			fixed4 _Color;
			float _Ratio0;
			float _Ratio1;
			float _Speed;
			float _Frequency0;
			float _Frequency1;

			fixed4 getGradient(const float _Phase)
			{
				const fixed4 color = FOREGROUND_BY_PHASE(0.5);
				fixed4 gradient = _ForegroundPrimaryColor;
				gradient = lerp(gradient, color, remap01Clamped(_Phase, _Ratio0, _Ratio1));
				gradient = lerp(gradient, _ForegroundPrimaryColor, remap01Clamped(_Phase, _Ratio1, 1));
				return gradient;
			}

			fragData vert(const vertData IN)
			{
				fragData OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv;
				OUT.mask = getUIMask(IN.vertex, OUT.vertex);
				OUT.color = IN.color * _Color;
				OUT.data.xy = IN.progress;
				OUT.data.y = 1 - OUT.data.y;
				
				const half2 screen = ComputeScreenPos(OUT.vertex);
				
				const float radians = IN.progress.x * UNITY_PI * 2 + floor(IN.size.x / 200);
				OUT.data.zw = half2(
					cos(radians * _Frequency0 + (_Time.y + screen.x) * 10) * 0.5 + 0.5,
					sin(radians * _Frequency1 + (_Time.y + screen.y) * 10) * 0.5 + 0.5
				);
				
				return OUT;
			}

			fixed4 frag(const fragData IN) : SV_Target
			{
				const half2 progress = IN.data.xy;
				const half2 wave     = IN.data.zw;
				const half  time     = frac(_Time.x * _Speed);
				const half  phase    = frac(progress.x - time);
				
				const fixed4 gradient = getGradient(phase);
				
				const half2 fogUV = half2(
					progress.x,
					progress.y * 0.3 - frac(_Time.x * 4)
				);
				
				const fixed4 fog = tex2D(_MaskTex, fogUV);
				
				fixed4 color = tex2D(_MainTex, IN.uv) * gradient * IN.color;
				
				color.a *= smoothstep(1, wave.x * 0.95, progress.y);
				color.a *= smoothstep(1, wave.y * 0.7, progress.y);
				color.a *= progress.y;
				color *= saturate(fog);
				
				color = FOREGROUND_BY_LUMINANCE(color);
				
				return useUIMask(color, IN.mask);
			}
			ENDCG
		}
	}
}
