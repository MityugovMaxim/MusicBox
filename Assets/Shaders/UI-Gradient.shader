Shader "UI/Gradient"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		_Color0 ("Color 0", Color) = (1,1,1,1)
		_Color1 ("Color 1", Color) = (1,1,1,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)
		_Color3 ("Color 3", Color) = (1,1,1,1)
		_Ratio0 ("Ratio 0", Range(0, 1)) = 0.333334
		_Ratio1 ("Ratio 1", Range(0, 1)) = 0.666667
		_Speed ("Speed", Range(0, 1)) = 1
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		
		_ColorMask ("Color Mask", Float) = 15
		_ClipRect ("Clip Rect", Vector) = (-32767, -32767, 32767, 32767)
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
		Blend SrcAlpha OneMinusSrcAlpha

		ColorMask [_ColorMask]

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
			#include "Math.cginc"
			#include "UIMask.cginc"
			
			struct vertData
			{
				float4 vertex   : POSITION;
				float4 color	: COLOR;
				float2 uv : TEXCOORD0;
			};

			struct fragData
			{
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				half2 uv  : TEXCOORD0;
				half4 mask : TEXCOORD1;
				half2 progress : TEXCOORD2;
			};
			
			fixed4 _Color;
			fixed4 _Color0;
			fixed4 _Color1;
			fixed4 _Color2;
			fixed4 _Color3;
			float _Ratio0;
			float _Ratio1;
			sampler2D _MainTex;
			float _Speed;

			fixed4 getGradient(const float _Phase)
			{
				fixed4 gradient = lerp(_Color0, _Color1, remap01Clamped(_Phase, 0, _Ratio0));
				gradient = lerp(gradient, _Color2, remap01Clamped(_Phase, _Ratio0, _Ratio1));
				gradient = lerp(gradient, _Color3, remap01Clamped(_Phase, _Ratio1, 1));
				return gradient;
			}

			fragData vert(vertData IN)
			{
				fragData OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv;
				OUT.mask = getUIMask(IN.vertex, OUT.vertex);
				OUT.color = IN.color * _Color;
				OUT.progress = ComputeScreenPos(OUT.vertex) * 0.5 + 0.5;
				return OUT;
			}

			fixed4 frag(fragData IN) : SV_Target
			{
				const half time  = frac(_Time.x * _Speed);
				const half phase = frac(IN.progress.x - time);
				
				const fixed4 gradient = getGradient(phase);
				
				const fixed4 color = tex2D(_MainTex, IN.uv) * gradient * IN.color;
				
				return useUIMask(color, IN.mask);
			}
		ENDCG
		}
	}
}
