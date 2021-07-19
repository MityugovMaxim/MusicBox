Shader "Spectrum/Lines"
{
	Properties
	{
		_ScaleX ("Scale", Float) = 0.98
		_ScaleY ("Scale", Float) = 0.95
		_Dampen ("Dampen", Range(0, 1)) = 0.1
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
			#include "Color.cginc"
			#include "UnityCustomRenderTexture.cginc"
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment frag

			float _Spectrum[64];
			fixed _ScaleX;
			fixed _ScaleY;
			fixed _Dampen;

			float4 frag(const v2f_customrendertexture IN) : COLOR
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
				
				const half2 uv = scale(IN.localTexcoord, half2(0.5, 0.5), half2(_ScaleX, _ScaleY));
				
				fixed4 color = tex2D(_SelfTexture2D, uv);
				color.a *= 1 - _Dampen;
				
				color = clamp(color + value, 0, 1);
				
				color = BACKGROUND_BY_RANGE(color, 0, 0.5);
				
				return color;
			}
			ENDCG
		}
	}
}