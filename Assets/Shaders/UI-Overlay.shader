// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UI/Overlay"
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
		_BlendSrc ("Blend Src", Int) = 0
		_BlendDst ("Blend Dst", Int) = 0
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
		Blend [_BlendSrc] [_BlendDst]
		ColorMask [_ColorMask]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UIMask.cginc"
			#include "UnityCG.cginc"

			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

			struct vertData
			{
				float4 vertex : POSITION;
				fixed4 color  : COLOR;
				half2 uv      : TEXCOORD0;
				float4 rect : TANGENT;
			};

			struct fragData
			{
				float4 vertex : SV_POSITION;
				fixed4 color  : COLOR;
				half2 uv      : TEXCOORD0;
				half4 mask    : TEXCOORD1;
				half4 rect : TANGENT;
				half4 position : TEXCOORD2;
			};

			sampler2D _MainTex;
			fixed4 _TextureSampleAdd;
			fixed4 _Color;

			fragData vert(const vertData IN)
			{
				fragData OUT;
				
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv     = IN.uv;
				OUT.color  = IN.color * _Color;
				OUT.mask   = getUIMask(IN.vertex, OUT.vertex);
				OUT.position = ComputeScreenPos(OUT.vertex);
				
				half2 min = IN.rect.xy;
				half2 max = IN.rect.zw;
				
				min = mul(UNITY_MATRIX_VP, min);
				max = mul(UNITY_MATRIX_VP, max);
				
				OUT.rect = half4(min.x, max.x, min.y, max.y);
				
				return OUT;
			}

			fixed4 frag(const fragData IN) : SV_Target
			{
				fixed left = smoothstep(IN.rect.x, IN.rect.x + 0.0001, IN.vertex.x);
				fixed right = smoothstep(IN.rect.y + 0.0001, IN.rect.y, IN.vertex.x);
				fixed top = smoothstep(IN.rect.w, IN.rect.w + 0.0001, IN.vertex.y);
				fixed bottom = smoothstep(IN.rect.w + 0.0001, IN.rect.w, IN.vertex.y);
				
				fixed value = left;
				
				return fixed4(value, value, value, 1);
				
				// if (IN.position.x < IN.rect.x)
				// 	discard;
				//
				// fixed4 color = tex2D(_MainTex, IN.uv) * IN.color;
				//
				// color = useUIMask(color, IN.mask);
				//
				// return color;
			}
			ENDCG
		}
	}
}