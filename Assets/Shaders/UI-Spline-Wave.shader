﻿Shader "UI/Spline/Wave"
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
				float4 vertex   : POSITION;
				fixed4 color    : COLOR;
				half2 uv        : TEXCOORD0;
				half2 fade      : TEXCOORD1;
				half4 rect      : TANGENT;
				float4 progress : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct fragData
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 uv        : TEXCOORD0;
				half4 mask      : TEXCOORD1;
				half2 fade      : TEXCOORD2;
				half2 wave      : TEXCOORD3;
				half4 rect      : TANGENT;
				float4 progress : NORMAL;
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
				UNITY_SETUP_INSTANCE_ID(IN);
				
				const half offset = frac(_Time.y * _Speed);
				
				OUT.vertex   = UnityObjectToClipPos(IN.vertex);
				#ifdef COLOR_SCHEME
				OUT.color    = IN.color * _ForegroundSecondaryColor;
				#else
				OUT.color    = IN.color;
				#endif
				OUT.uv       = IN.rect.xy + IN.rect.zw * IN.uv.xy;
				OUT.fade     = IN.fade;
				OUT.wave     = ComputeScreenPos(OUT.vertex).xy * _Scale - offset;
				OUT.rect     = IN.rect;
				OUT.progress = IN.progress;
				OUT.mask     = getUIMask(OUT.vertex.w, IN.vertex.xy);
				
				return OUT;
			}

			fixed4 frag(const fragData IN) : SV_Target
			{
				const half3 wave = UnpackNormal(tex2D(_WaveTex, IN.wave)) * _Strength;
				const half2 uv   = IN.uv + wave.xy * IN.rect.zz;
				
				fixed4 color = (tex2D(_MainTex, uv) + _TextureSampleAdd) * IN.color;
				
				color.rgb *= color.a;
				
				color += grayscale(color);
				
				#ifdef UNITY_UI_CLIP_RECT
				half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
				color.a *= m.x * m.y;
				#endif
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif
				
				color.a *= smoothstep(IN.progress.x, IN.progress.x + IN.fade.x, IN.progress.z);
				color.a *= smoothstep(IN.progress.y, IN.progress.y - IN.fade.y, IN.progress.z);
				
				return color;
			}
			ENDCG
		}
	}
}