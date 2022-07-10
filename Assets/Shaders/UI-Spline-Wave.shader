Shader "UI/Spline/Wave"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		
		_WaveTex ("Wave Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_Strength ("Strength", Float) = 1
		_Speed ("Speed", Float) = 1
		_Scale ("Scale", Float) = 1
		
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		[Toggle(COLOR_SCHEME)] _UseColorScheme ("Use Color Scheme", Float) = 0
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

			#include "Color.cginc"
			#include "Math.cginc"
			#include "UIMask.cginc"
			#include "UnityCG.cginc"

			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP
			#pragma multi_compile_local _ COLOR_SCHEME

			struct vertData
			{
				float4 vertex : POSITION;
				fixed4 color  : COLOR;
				half2 uv      : TEXCOORD0;
			};

			struct fragData
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 uv        : TEXCOORD0;
				half2 wave      : TEXCOORD2;
			};

			sampler2D _MainTex;
			sampler2D _WaveTex;
			fixed4 _Color;
			fixed4 _TextureSampleAdd;
			half _Strength;
			half _Speed;
			half _Scale;

			fragData vert(const vertData IN)
			{
				fragData OUT;
				
				const half offset = frac(_Time.y * _Speed);
				
				OUT.vertex   = UnityObjectToClipPos(IN.vertex);
				
				OUT.color = IN.color;
				OUT.uv    = IN.uv;
				OUT.wave  = ComputeScreenPos(OUT.vertex).xy * _Scale - offset;
				
				return OUT;
			}

			fixed4 frag(const fragData IN) : SV_Target
			{
				const half3 wave = UnpackNormal(tex2D(_WaveTex, IN.wave)) * _Strength;
				const half2 uv   = IN.uv + wave.xy;
				
				fixed4 color = tex2D(_MainTex, uv) * IN.color;
				
				// #ifdef COLOR_SCHEME
				// color = BACKGROUND_BY_GRAYSCALE(color);
				// #endif
				
				//const fixed phase = grayscale(color);
				
				//color.rgb *= phase;
				
				//color += grayscale(color);
				
				//color *= 6;
				
				//color.rgb += smoothstep(0.6, 0.8, phase) * 4;
				
				return color * color;
			}
			ENDCG
		}
	}
}