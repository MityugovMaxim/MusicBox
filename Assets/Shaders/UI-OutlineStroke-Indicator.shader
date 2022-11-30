Shader "UI/OutlineStroke Indicator"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_Speed ("Speed", Float) = 1
		
		_Ratio0 ("Ratio 0", Range(0, 1)) = 0.333334
		_Ratio1 ("Ratio 1", Range(0, 1)) = 0.666667
		
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		[Toggle(BACKGROUND_SCHEME)] _UseBackgroundScheme ("Use Background Scheme", Float) = 0
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
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UIMask.cginc"
			#include "UnityCG.cginc"
			#include "Math.cginc"
			#include "Color.cginc"

			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP
			#pragma multi_compile_local _ BACKGROUND_SCHEME

			struct vertData
			{
				float4 vertex : POSITION;
				float4 color  : COLOR;
				float2 uv     : TEXCOORD0;
				float2 progress : TEXCOORD1;
				float2 size : TEXCOORD2;
			};

			struct fragData
			{
				float4 vertex : SV_POSITION;
				fixed4 color  : COLOR;
				float2 grid     : TEXCOORD0;
				half4  mask   : TEXCOORD1;
				float2 progress : TEXCOORD2;
				float2 size : TEXCOORD3;
			};

			sampler2D _MainTex;
			fixed4 _TextureSampleAdd;
			fixed4 _Color;
			float _Ratio0;
			float _Ratio1;
			float _Speed;

			fixed4 getGradient(const fixed _Alpha, const float _Phase)
			{
				const fixed4 color0 = FOREGROUND_BY_PHASE(_Alpha);
				const fixed4 color1 = FOREGROUND_BY_PHASE(_Alpha * 0.4);
				
				fixed4 gradient = color0;
				gradient = lerp(gradient, color1, remap01Clamped(_Phase, _Ratio0, _Ratio1));
				gradient = lerp(gradient, color0, remap01Clamped(_Phase, _Ratio1, 1));
				return gradient;
			}

			fragData vert(const vertData IN)
			{
				fragData OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.grid = IN.uv;
				OUT.mask = getUIMask(IN.vertex, OUT.vertex);
				OUT.color = IN.color * _Color;
				OUT.progress = IN.progress;
				OUT.size = IN.size;
				return OUT;
			}

			fixed4 frag(const fragData IN) : SV_Target
			{
				const half time  = frac(_Time.x * _Speed);
				const half phase = frac(IN.progress.x - time);
				
				const fixed fade = 0.25;
				const float size = IN.size.y;
				const float position = size * IN.progress.y;
				
				fixed4 color = tex2D(_MainTex, IN.grid);
				color *= IN.color;
				color *= getGradient(color.a, phase);
				color.a *= smoothstep(0, fade, position) * smoothstep(size, size - fade, position);
				
				return useUIMask(color, IN.mask);
			}
			ENDCG
		}
	}
}
