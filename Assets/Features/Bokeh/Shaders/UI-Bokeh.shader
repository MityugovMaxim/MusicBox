Shader "UI/Bokeh"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" { }
		_Color ("Tint", Color) = (1,1,1,1)
		_Speed ("Speed", Float) = 0
		_Amplitude ("Amplitude", Float) = 0
		
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
		Blend SrcAlpha One
		ColorMask [_ColorMask]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Assets/Shaders/UIMask.cginc"
			#include "UnityCG.cginc"

			struct vertData
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				half2 uv : TEXCOORD0;
			};

			struct fragData
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				half2 uv : TEXCOORD0;
				half4 mask : TEXCOORD1;
			};

			sampler2D _MainTex;
			fixed4 _Color;

			fragData vert (const vertData IN)
			{
				fragData OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				const float phase = smoothstep(0.2, 0.8, length(OUT.vertex.xy) * 0.75);
				OUT.uv = IN.uv;
				OUT.color = IN.color * _Color * phase + phase * 0.25;
				OUT.mask = getUIMask(IN.vertex, OUT.vertex);
				return OUT;
			}

			fixed4 frag (const fragData IN) : SV_Target
			{
				const fixed4 color = tex2D(_MainTex, IN.uv);
				
				return useUIMask(color, IN.mask) * IN.color;
			}
			ENDCG
		}
	}
}
