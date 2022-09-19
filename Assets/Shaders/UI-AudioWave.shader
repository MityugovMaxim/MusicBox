Shader "UI/AudioWave"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "black" { }
		_Color ("Color", Color) = (1, 1, 1, 1)
		
		[HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
		[HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
		[HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
		[HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
		[HideInInspector] _ColorMask ("Color Mask", Float) = 15
		[HideInInspector] _BlendSrc ("Blend Src", Int) = 0
		[HideInInspector] _BlendDst ("Blend Dst", Int) = 0
		
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
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

		Blend SrcAlpha OneMinusSrcAlpha

		ColorMask [_ColorMask]

		Pass
		{
			Name "Default"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile __ UNITY_UI_CLIP_RECT
			#pragma multi_compile __ UNITY_UI_ALPHACLIP

			#include "UnityCG.cginc"
			#include "UIMask.cginc"

			struct vertData
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
				float2 time : TEXCOORD2;
				float2 direction : TEXCOORD3;
			};

			struct fragData
			{
				float4 vertex : SV_POSITION;
				fixed4 color  : COLOR;
				float4 data   : TEXCOORD0;
				half4 mask    : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			fixed4 _Color;

			float2 GetCoord(const float _Sample, const float _Size)
			{
				const float x = _Sample % _Size;
				const float y = _Sample / _Size;
				return float2(x / _Size, y / _Size);
			}

			fixed4 GetData(const sampler2D _Data, const float _Sample, const float _Size)
			{
				const float minSample = floor(_Sample);
				const float maxSample = ceil(_Sample);
				
				const float phase = frac(_Sample);
				
				const fixed4 minData = tex2D(_Data, GetCoord(minSample, _Size));
				const fixed4 maxData = tex2D(_Data, GetCoord(maxSample, _Size));
				
				return lerp(minData, maxData, phase);
			}

			float GetSample(const float2 _Time, const half2 _UV, const half2 _Direction)
			{
				return lerp(_Time.x, _Time.y, dot(_Direction, _UV));
			}

			float GetPosition(const half2 _UV, const half2 _Direction)
			{
				return dot(float2(1, 1) - _Direction, _UV);
			}

			fragData vert(const vertData IN)
			{
				const float size     = _MainTex_TexelSize.z;
				const float sample   = GetSample(IN.time, IN.uv, IN.direction);
				const float position = GetPosition(IN.uv, IN.direction);
				
				fragData OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.color = IN.color * _Color;
				OUT.mask = getUIMask(IN.vertex, OUT.vertex);
				OUT.data = float4(size, sample, position, 0);
				return OUT;
			}

			fixed4 frag(fragData IN) : SV_Target
			{
				const float size     = IN.data.x;
				const float sample   = IN.data.y;
				const float position = abs(IN.data.z * 2 - 1);
				
				const fixed4 data = GetData(_MainTex, sample, size);
				
				const float maxValue = data.r;
				const float avgValue = data.g;
				const float rmsValue = data.b;
				
				const float smooth = 0.01;
				
				const float maxThreshold = smoothstep(maxValue + smooth, maxValue, position);
				const float avgThreshold = smoothstep(avgValue + smooth, avgValue, position);
				const float rmsThreshold = smoothstep(rmsValue + smooth, rmsValue, position);
				
				fixed4 color = fixed4(IN.color.rgb, 0);
				
				// Max color
				color.a = lerp(color.a, 1, maxThreshold);
				
				// RMS color
				color.rgb = lerp(color.rgb, IN.color * 1.5, rmsThreshold);
				
				// Avg color
				color.rgb = lerp(color.rgb, IN.color * 2, avgThreshold);
				
				color = useUIMask(color, IN.mask);
				
				return color;
			}
			ENDCG
		}
	}
}