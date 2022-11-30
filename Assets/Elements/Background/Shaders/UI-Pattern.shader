Shader "UI/Pattern"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_SourceColor ("Source", Color) = (1,1,1,1)
		_TargetColor ("Target", Color) = (1,1,1,1)
		_SourceGradient ("Source Gradient", Color) = (1,1,1,1)
		_TargetGradient ("Target Gradient", Color) = (1,1,1,1)
		
		_OutRadius("Out Radius", Float) = 0
		_InRadius("In Radius", Float) = 0
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

			#include "UnityCG.cginc"
			#include "Assets/Shaders/Math.cginc"

			struct vertData
			{
				float4 vertex : POSITION;
				float4 color  : COLOR;
				half2 uv      : TEXCOORD0;
				half2 mask    : TEXCOORD1;
				half2 data    : TEXCOORD2;
			};

			struct fragData
			{
				float4 vertex  : SV_POSITION;
				fixed4 color   : COLOR;
				float2 uv      : TEXCOORD0;
				float2 grid      : TEXCOORD1;
			};

			sampler2D _MainTex;
			fixed4 _TextureSampleAdd;
			fixed4 _SourceColor;
			fixed4 _TargetColor;
			fixed4 _SourceGradient;
			fixed4 _TargetGradient;
			float _InRadius;
			float _OutRadius;
			float _Smooth;

			fragData vert(const vertData IN)
			{
				fragData OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				
				const int count = 30;
				const half2 uv = (IN.uv * 2 - 1) / half2(1, _ScreenParams.x / _ScreenParams.y);

				OUT.uv = IN.uv;
				OUT.grid = rotate45(uv, half2(0, 0)) * count;
				OUT.color = IN.color;
				return OUT;
			}

			fixed4 frag(const fragData IN) : SV_Target
			{
				const half2 uv = frac(IN.grid);
				const half2 id = floor(IN.grid);
				
				const float time = frac(_Time.x * 4);
				const half fade = smoothstep(0, 0.4, time) * smoothstep(1, 0.6, time);
				const float radius = time * 45;
				const float thickness = 20;
				const float ring = getRing(id + half2(0.5, 0.5), radius, radius - thickness, _Smooth) * fade;
				const float size = lerp(2, 1, ring);
				
				const fixed4 dot = tex2D(_MainTex, scale(uv, half2(0.5, 0.5), size));
				
				const fixed grayscale = (dot.r + dot.g + dot.b) * 0.333333;
				
				const fixed background = remap(noise(id, 5), 0, 1, 0.15, 0.5);
				
				fixed value = grayscale;
				value *= background;
				value += grayscale * ring;
				value += grayscale * ring * background;
				value += grayscale * ring * smoothstep(0.275, 0.29, background) * 3.5;
				
				fixed4 color = value * lerp(_SourceColor, _TargetColor, smoothstep(0.975, 1, value));
				color.a = 1;
				
				const float2 wave = float2(
					0.5 + sin(IN.uv.x * 3 + _Time.y * 1) * 0.15,
					0.5 + sin(IN.uv.y * 6 + _Time.y * 0.5) * 0.1
				);
				
				const float phase = getCircle(IN.uv - wave, 0.6, 0.6);
				
				const fixed4 gradient = lerp(_SourceGradient, _TargetGradient, phase);
				
				return (gradient + color) * IN.color;
			}
			ENDCG
		}
	}
}
