Shader "UI/Dissolve"
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
			#pragma multi_compile _ BACKGROUND_SCHEME FOREGROUND_SCHEME

			#include "UIMask.cginc"
			#include "UnityCG.cginc"

			#ifdef BACKGROUND_SCHEME
			#include "Color.cginc"
			#elif FOREGROUND_SCHEME
			#include "Color.cginc"
			#endif

			struct vertData
			{
				float4 vertex : POSITION;
				float4 color  : COLOR;
				half2 uv      : TEXCOORD0;
				half2 mask    : TEXCOORD1;
				half2 data    : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct fragData
			{
				float4 vertex  : SV_POSITION;
				fixed4 color   : COLOR;
				float2 uv      : TEXCOORD0;
				half4  mask    : TEXCOORD1;
				half4 dissolve : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			sampler2D _DissolveTex;
			fixed4 _TextureSampleAdd;
			fixed4 _Color;

			fragData vert(const vertData IN)
			{
				fragData OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv;
				OUT.mask = getUIMask(OUT.vertex.w, IN.vertex.xy);
				OUT.dissolve = half4(IN.mask.xy, IN.data.xy);
				OUT.color = IN.color * _Color;
				return OUT;
			}

			fixed4 frag(const fragData IN) : SV_Target
			{
				const fixed threshold = IN.dissolve.z;
				const fixed border = IN.dissolve.w;
				const fixed dissolve = tex2D(_DissolveTex, IN.dissolve.xy).a;
				half4 color = IN.color * (tex2D(_MainTex, IN.uv) + _TextureSampleAdd);
				
				#ifdef UNITY_UI_CLIP_RECT
				half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
				color.a *= m.x * m.y;
				#endif
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif
				
				const fixed phase = smoothstep(threshold + border, threshold, dissolve);
				color *= phase;
				#ifdef BACKGROUND_SCHEME
				color *= BACKGROUND_BY_PHASE(phase);
				#elif FOREGROUND_SCHEME
				color *= FOREGROUND_BY_PHASE(phase);
				#endif
				
				return color;
			}
			ENDCG
		}
	}
}
