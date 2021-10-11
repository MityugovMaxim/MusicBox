Shader "UI/Cube"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_NormalTex ("Normal Texture", 2D) = "black" {}
		_ReflectionTex ("Reflection Texture", 2D) = "black" {}
		_Refraction ("Refraction", Float) = 0.5
		_Strength ("Strength", Float) = 1
		_SpeedX ("Speed X", Float) = 1
		_SpeedY ("Speed Y", Float) = 1
		_Scale ("Scale", Float) = 1
		
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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

			#include "UnityCG.cginc"
			#include "UIMask.cginc"
			#include "Math.cginc"

			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

			struct vertData
			{
				float4 vertex : POSITION;
				fixed4 color  : COLOR;
				half2 uv      : TEXCOORD0;
				half2 mask    : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct fragData
			{
				float4 vertex : SV_POSITION;
				fixed4 color  : COLOR;
				half2 uv      : TEXCOORD0;
				half4 mask    : TEXCOORD1;
				half2 screen  : TEXCOORD2;
			};

			sampler2D _MainTex;
			sampler2D _NormalTex;
			sampler2D _ReflectionTex;
			fixed4 _Color;
			half _Refraction;
			half _Strength;
			float _Speed;
			float _SpeedX;
			float _SpeedY;
			float _Scale;

			fragData vert(const vertData IN)
			{
				fragData OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				
				const half2 offset = half2(
					frac(_Time.x * _SpeedX),
					frac(_Time.x * _SpeedY)
				);
				
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.color = IN.color * _Color;
				OUT.uv = IN.uv;
				OUT.mask = getUIMask(OUT.vertex.w, IN.vertex.xy);
				OUT.screen = ComputeScreenPos(OUT.vertex) * _Scale - offset;
				
				return OUT;
			}

			fixed4 frag(const fragData IN) : SV_Target
			{
				const fixed4 normal = tex2D(_NormalTex, IN.uv);
				const fixed2 offset = (normal.xy * 2 - 1) * normal.a * _Refraction;
				const fixed4 reflection = tex2D(_ReflectionTex, IN.screen - offset);
				
				const fixed3 reflectionColor = clamp(reflection.rgb, 0, 1);
				
				fixed4 color = tex2D(_MainTex, IN.uv) * IN.color;
				color.rgb += color.rgb * reflectionColor * reflectionColor * normal.a * 60 * _Strength;
				
				#ifdef UNITY_UI_CLIP_RECT
				half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
				color.a *= m.x * m.y;
				#endif
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif
				
				return color;
			}
			ENDCG
		}
	}
}
