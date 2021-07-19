﻿Shader "Spectrum/Lines"
{
	Properties
	{
		_SourceColor ("Source", Color) = (1,1,1,1)
		_TargetColor ("Source", Color) = (1,1,1,1)
		_Scale ("Scale", Float) = 0.1
	}

	 SubShader
	 {
		Lighting Off
	 	Cull Off
		Blend One Zero

		Pass
		{
			CGPROGRAM
			#include "Math.cginc"
			#include "UnityCustomRenderTexture.cginc"
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment frag

			float _Spectrum[64];
			fixed4 _SourceColor;
			fixed4 _TargetColor;
			fixed4 _TraceColor;
			fixed _Scale;

			float4 frag(v2f_customrendertexture IN) : COLOR
			{
				const half aspect = _ScreenParams.x / _ScreenParams.y;
				half2 base = IN.localTexcoord;
				base.y /= aspect;
				
				const half size = 63;
				
				const half step   = 1.0 / size;
				const half offset = 0.35 / aspect;
				
				const int cIndex = floor(base.x * size);
				const int lIndex = max(0, cIndex - 1);
				const int rIndex = min(cIndex + 1, size - 1);
				
				const half2 cPosition = half2(step * 0.5 + step * cIndex, _Spectrum[cIndex] * 0.5 + offset);
				const half2 lPosition = half2(step * 0.5 + step * lIndex, _Spectrum[lIndex] * 0.5 + offset);
				const half2 rPosition = half2(step * 0.5 + step * rIndex, _Spectrum[rIndex] * 0.5 + offset);
				
				fixed value = 0;
				value += getLine(base, cPosition, lPosition, 0.008, 0.008);
				value += getLine(base, cPosition, rPosition, 0.008, 0.008);
				
				fixed4 bend = lerp(_SourceColor, _TargetColor, value);
				bend.a = value;
				
				const half2 uv = scale(IN.localTexcoord, half2(0.5, 0.5), half2(0.98, 0.95));
				
				fixed4 color = tex2D(_SelfTexture2D, uv);
				
				const float average = (color.r + color.g + color.b) * 0.333333;
				
				color *= lerp(
					fixed4(0.32, 0.64, 0.72, 0.8),
					fixed4(0.72, 0.10, 0.23, 0.8),
					average
				);
				
				color += bend;
				
				return color;
			}
			ENDCG
		}
	}
}