Shader "UI/Spline/Default"
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

			#include "UIMask.cginc"
			#include "UnityCG.cginc"

			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

			struct vertData
			{
				float4 vertex : POSITION;
				fixed4 color  : COLOR;
				half2 uv      : TEXCOORD0;
				half4 rect    : TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct fragData
			{
				float4 vertex : SV_POSITION;
				fixed4 color  : COLOR;
				half2 uv      : TEXCOORD0;
				half4 mask    : TEXCOORD1;
				half4 rect    : TANGENT;
			};

			sampler2D _MainTex;
			fixed4 _Color;
			fixed4 _TextureSampleAdd;

			fragData vert(const vertData IN)
			{
				fragData OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.color  = IN.color * _Color;
				OUT.uv     = IN.rect.xy + IN.rect.zw * IN.uv;
				OUT.rect   = IN.rect;
				OUT.mask   = getUIMask(OUT.vertex.w, IN.vertex.xy);
				
				return OUT;
			}

			fixed4 frag(const fragData IN) : SV_Target
			{
				fixed4 color = (tex2D(_MainTex, IN.uv) + _TextureSampleAdd) * IN.color;
				
				color = useUIMask(color, IN.mask);
				
				return color;
			}
			ENDCG
		}
	}
}