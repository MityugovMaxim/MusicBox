Shader "UI/Laser"
{
	Properties
	{
		_BeamTex ("Beam", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_Speed ("Speed", Float) = 1
		_Size ("Size", Float) = 1
		
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
			"RenderType" = "Transparent"
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

			#include "UnityCG.cginc"
			#include "Math.cginc"
			#include "Color.cginc"

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
				half2 fog : TEXCOORD1;
			};

			sampler2D _BeamTex;
			float _Speed;
			float _Size;
			fixed4 _Color;

			fragData vert(const vertData IN)
			{
				fragData OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.color = IN.color * _Color;
				OUT.uv = IN.uv;
				OUT.fog = ComputeScreenPos(OUT.vertex) * _Size - half2(0, frac(_Time.x * _Speed));
				return OUT;
			}

			fixed4 frag(const fragData IN) : SV_Target
			{
				fixed color = saturate(tex2D(_BeamTex, IN.uv));
				
				color = remap(color, 0, 1, 1, 0.8);
				
				return fixed4(1, 1, 1, color) * IN.color * _BackgroundPrimaryColor;
			}
			ENDCG
		}
	}
}
