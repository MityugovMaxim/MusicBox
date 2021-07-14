﻿Shader "UI/Progress"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		
		_Color ("Tint", Color) = (1,1,1,1)
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		
		[HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
		[HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
		[HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
		[HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
		[HideInInspector] _ColorMask ("Color Mask", Float) = 15
		[HideInInspector] _SrcBlend ("Src Blend", Int) = 0
		[HideInInspector] _DstBlend ("Dst Blend", Int) = 0
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
		Blend [_SrcBlend] [_DstBlend]
		ColorMask [_ColorMask]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

			struct vertData
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 uv       : TEXCOORD0;
				float2 fade     : TEXCOORD1;
				float4 progress : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct fragData
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 uv       : TEXCOORD0;
				half4 mask      : TEXCOORD1;
				float2 fade     : TEXCOORD2;
				float4 progress : NORMAL;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			float4 _ClipRect;
			fixed4 _TextureSampleAdd;
			float _UIMaskSoftnessX;
			float _UIMaskSoftnessY;

			fragData vert(vertData IN)
			{
				fragData OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.progress = IN.progress;
				OUT.fade = IN.fade;
				OUT.color = IN.color * _Color;
				OUT.uv = TRANSFORM_TEX(IN.uv.xy, _MainTex);
				
				float2 pixelSize = OUT.vertex.w / float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
				float4 rect = clamp(_ClipRect, -2e10, 2e10);
				OUT.mask = half4(
					IN.vertex.xy * 2 - rect.xy - rect.zw,
					0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy))
				);
				
				return OUT;
			}

			fixed4 frag(fragData IN) : SV_Target
			{
				half4 color = (tex2D(_MainTex, IN.uv) + _TextureSampleAdd) * IN.color;
				
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