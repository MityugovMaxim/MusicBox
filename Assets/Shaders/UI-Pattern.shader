Shader "UI/Pattern"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		_OutRadius("Out Rad", Float) = 0
		_InRadius("In Rad", Float) = 0
		_Smooth("Smooth", Float) = 0
		
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

			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP
			#pragma multi_compile _ BACKGROUND_SCHEME FOREGROUND_SCHEME

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
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			fixed4 _TextureSampleAdd;
			fixed4 _Color;
			float _InRadius;
			float _OutRadius;
			float _Smooth;

			fragData vert(const vertData IN)
			{
				fragData OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				
				const int count = 15;
				const half2 uv = (IN.uv * 2 - 1) / half2(1, _ScreenParams.x / _ScreenParams.y);
				
				OUT.uv = rotate45(uv, half2(0, 0)) * count;
				OUT.mask = getUIMask(OUT.vertex.w, IN.vertex.xy);
				OUT.color = IN.color * _Color;
				return OUT;
			}

			fixed4 frag(const fragData IN) : SV_Target
			{
				const half2 uv = frac(IN.uv);
				const half2 id = floor(IN.uv);
				
				const float val = remap(noise(id, 5), 0, 1, 0.02, 0.07);
				
				const float time = frac(_Time.x * 5);
				const half fadeIn = smoothstep(0, 0.2, time);
				const half fadeOut = smoothstep(1, 0.8, time);
				const float radius = time * 40;
				const float ring = getRing(id, radius, radius - 10, _Smooth) * fadeIn * fadeOut;
				
				fixed4 color = tex2D(_MainTex, uv) * IN.color;
				color.rgb *= color.a * val + color.a * ring * val * 5 + ring * smoothstep(0.065, 0.07, val);
				
				#ifdef UNITY_UI_CLIP_RECT
				half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
				color.a *= m.x * m.y;
				#endif
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif
				
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
