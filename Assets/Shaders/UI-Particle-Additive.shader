Shader "UI/Particle/Additive"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		[Toggle(BACKGROUND_SCHEME)] _UseBackgroundScheme ("Use Background Scheme", Float) = 0
		[HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
		[HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
		[HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
		[HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
		[HideInInspector] _ColorMask ("Color Mask", Float) = 15
		[HideInInspector] _ClipRect ("Clip Rect", Vector) = (-32767, -32767, 32767, 32767)
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
		Fog { Mode Off }
		Blend SrcAlpha One

		ColorMask [_ColorMask]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Color.cginc"
			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile_local _ BACKGROUND_SCHEME

			struct vertData
			{
				float4 vertex : POSITION;
				float4 color  : COLOR;
				float2 uv     : TEXCOORD0;
			};

			struct fragData
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 uv        : TEXCOORD0;
				float4 position : TEXCOORD1;
			};

			sampler2D _MainTex;
			fixed4 _Color;
			float4 _MainTex_ST;
			float4 _ClipRect;

			fragData vert(const vertData IN)
			{
				fragData OUT;
				OUT.position = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
				#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw - 1.0) * float2(-1, 1);
				#endif
				OUT.color = IN.color * _Color;
				return OUT;
			}

			fixed4 frag(const fragData IN) : SV_Target
			{
				fixed4 color = tex2D(_MainTex, IN.uv) * IN.color;
				
				color.a *= UnityGet2DClipping(IN.position.xy, _ClipRect);
				
				#ifdef BACKGROUND_SCHEME
				color *= BACKGROUND_BY_LUMINANCE(color);
				#else
				color *= FOREGROUND_BY_LUMINANCE(color);
				#endif
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.01);
				#endif
				
				return color;
			}
			ENDCG
		}
	}
}
