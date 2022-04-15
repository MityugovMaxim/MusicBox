Shader "UI/Grayscale"
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
		[HideInInspector] _BlendSrc ("Blend Src", Int) = 0
		[HideInInspector] _BlendDst ("Blend Dst", Int) = 0
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

			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP
			#pragma multi_compile_local _ BACKGROUND_SCHEME FOREGROUND_SCHEME

			#include "UIMask.cginc"
			#include "UnityCG.cginc"
			#include "Math.cginc"

			#ifdef BACKGROUND_SCHEME
			#include "Color.cginc"
			#elif FOREGROUND_SCHEME
			#include "Color.cginc"
			#endif

			struct vertData
			{
				float4 vertex : POSITION;
				float4 color  : COLOR;
				half2  uv     : TEXCOORD0;
				half2  rect   : TEXCOORD1;
				half2  data   : TEXCOORD2;
			};

			struct fragData
			{
				float4 vertex : SV_POSITION;
				fixed4 color  : COLOR;
				half2  uv     : TEXCOORD0;
				half2  rect   : TEXCOORD2;
				half2  data   : TEXCOORD1;
				half4  mask   : TEXCOORD3;
			};

			sampler2D _MainTex;
			fixed4 _TextureSampleAdd;
			fixed4 _Color;

			fragData vert(const vertData IN)
			{
				fragData OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.color = IN.color * _Color;
				OUT.uv = IN.uv;
				OUT.rect = IN.rect;
				OUT.data = IN.data;
				OUT.mask = getUIMask(IN.vertex, OUT.vertex);
				return OUT;
			}

			fixed4 frag(const fragData IN) : SV_Target
			{
				fixed4 color = IN.color * (tex2D(_MainTex, IN.uv) + _TextureSampleAdd);
				
				const fixed grayscale = 0.2126 * color.r + 0.7152 * color.g + 0.0722 * color.b;
				
				color.rgb = lerp(color.rgb, grayscale * IN.data.y, IN.data.x);
				
				#ifdef BACKGROUND_SCHEME
				color.rgb *= BACKGROUND_BY_RANGE(color, 0.15, 0.8);
				#elif FOREGROUND_SCHEME
				color.rgb *= FOREGROUND_BY_RANGE(color, 0.15, 0.8);
				#endif
				
				color = useUIMask(color, IN.mask);
				
				return color;
			}
			ENDCG
		}
	}
}
