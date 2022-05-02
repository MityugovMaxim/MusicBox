Shader "UI/AudioWave"
{
	Properties
	{
		_BackgroundColor ("Background Color", Color) = (0, 0, 0, 1)
		_MaxSamples ("Max Samples", 2D) = "black" {}
		_AvgSamples ("Avg Samples", 2D) = "black" {}
		_MaxColor ("Max Color", Color) = (1, 1, 1, 1)
		_AvgColor ("Avg Color", Color) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}

		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Color.cginc"

			struct vertData
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct fragData
			{
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
				float4 vertex : SV_POSITION;
			};

			fixed4 _BackgroundColor;
			fixed4 _MaxColor;
			fixed4 _AvgColor;
			int    _MinTime;
			int    _MaxTime;
			int    _Size;

			sampler2D _MaxSamples;
			sampler2D _AvgSamples;

			float2 GetCoord(const int _Index, const int _Size)
			{
				const float x = _Index % _Size;
				const float y = _Index / _Size;
				return float2(x / _Size, y / _Size);
			}

			float GetValue(const sampler2D _Samples, const int _Length, const float _Data, const float _Limit)
			{
				if (_Length < 1)
					return _Limit;
				
				const int minIndex = floor(_Data);
				if (minIndex < 0 || minIndex >= _Length)
					return _Limit;
				
				const int maxIndex = ceil(_Data);
				if (maxIndex < 0 || maxIndex >= _Length)
					return _Limit;
				
				const half phase = frac(_Data);
				
				const float minValue = tex2D(_Samples, GetCoord(minIndex, _Size)).a;
				const float maxValue = tex2D(_Samples, GetCoord(maxIndex, _Size)).a;
				
				const float value = lerp(minValue, maxValue, phase);
				
				return max(_Limit, value * 0.75);
			}

			fragData vert(vertData IN)
			{
				const int index = lerp(_MinTime, _MaxTime, IN.uv.y);
				
				fragData OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = float2(IN.uv.x * 2 - 1, index);
				OUT.color = IN.color;
				return OUT;
			}

			fixed4 frag(fragData IN) : SV_Target
			{
				const float data = IN.uv.y;
				const float position = abs(IN.uv.x);
				
				const float maxValue = GetValue(_MaxSamples, _Size * _Size, data, 0.002);
				const float avgValue = GetValue(_AvgSamples, _Size * _Size, data, 0.001);
				
				const float maxThreshold = smoothstep(maxValue + 0.002, maxValue, position);
				const float avgThreshold = smoothstep(avgValue + 0.002, avgValue, position);
				
				const fixed max = smoothstep(maxValue, avgValue, position);
				const fixed avg = smoothstep(avgValue, 0, position);
				
				fixed4 maxColor = lerp(_BackgroundSecondaryColor, _BackgroundPrimaryColor, max) * 0.4;
				maxColor.a = maxThreshold;
				
				fixed4 avgColor = lerp(_ForegroundPrimaryColor, _ForegroundSecondaryColor, avg) * 0.4;
				avgColor.a = avgThreshold;
				
				fixed4 color = _BackgroundColor;
				color = lerp(color, maxColor, maxColor.a);
				color = lerp(color, avgColor, avgColor.a);
				
				return color * IN.color;
			}
			ENDCG
		}
	}
}