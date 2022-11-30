Shader "Elements/Background"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SourceColor ("Source Color", Color) = (1, 1, 1, 1)
		_TargetColor ("Target Color", Color) = (1, 1, 1, 1)
		_Radius ("Radius", Range(0, 1)) = 0.5
		_Smooth ("Smooth", Range(0, 1)) = 0.5
		
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
			#include "Assets/Shaders/Math.cginc"

			struct vertData
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct fragData
			{
				float2 grid : TEXCOORD0;
				float2 phase : TEXCOORD1;
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			fixed4 _SourceColor;
			fixed4 _TargetColor;
			float _Radius;
			float _Smooth;

			fragData vert (const vertData IN)
			{
				fragData OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.grid = IN.uv;
				OUT.color = IN.color;
				OUT.phase = float2(
					_SinTime.y * 0.5 + 0.5,
					_SinTime.z * 0.5 + 0.5
				);
				return OUT;
			}

			fixed4 frag (const fragData IN) : SV_Target
			{
				const fixed4 color = tex2D(_MainTex, IN.grid);
				
				const half2 offset = half2(0.5 + IN.phase.x * 0.1, 0.5);
				
				const float pulse = IN.phase.y * 0.2;
				
				const float phase = getCircle(IN.grid - offset, _Radius + pulse, _Smooth + pulse);
				
				const fixed4 gradient = lerp(_SourceColor, _TargetColor, phase);
				
				return color * gradient * IN.color;
			}
			ENDCG
		}
	}
}
