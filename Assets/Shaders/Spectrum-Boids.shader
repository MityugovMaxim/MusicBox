Shader "Spectrum/Boids"
{
	Properties
	{
		_Scale ("Scale", Float) = 0.95
		_Dampen ("Dampen", Range(0, 1)) = 0.25
		_DotRadius ("Dot Radius", Range(0, 0.2)) = 0.06
		_DotSmooth ("Dot Smooth", Range(0, 0.2)) = 0.03
		_HighlightRadius ("Highlight Radius", Range(0, 0.2)) = 0.02
		_HighlightSmooth ("Highlight Smooth", Range(0, 0.2)) = 0.01
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
			fixed _Scale;
			fixed _Dampen;
			fixed _DotRadius;
			fixed _DotSmooth;
			fixed _HighlightRadius;
			fixed _HighlightSmooth;

			float4 frag(const v2f_customrendertexture IN) : COLOR
			{
				half2 base = IN.localTexcoord * 2 - 1;
				base.y /= _ScreenParams.x / _ScreenParams.y;
				base = rotate(base, -_Time.y);
				
				const int count = 64;
				
				const half2 origin = rotate270(base);
				
				const fixed angle = 1 - (UNITY_PI + atan2(origin.x, origin.y)) / UNITY_TWO_PI;
				
				const half index = floor(angle * count);
				const fixed phase = index / count;
				
				const half offset = UNITY_PI / count;
				const half radians = UNITY_TWO_PI * phase + offset;
				const fixed distance = 0.45 + _Spectrum[index] * 0.5;
				
				const half2 position = rotate(half2(1, 0), radians) * distance - base;
				
				const fixed dot = getCircle(position, _DotRadius, _DotSmooth);
				const fixed highlight = getCircle(position, _HighlightRadius, _HighlightSmooth);
				
				const half2 uv = scale(
					IN.localTexcoord,
					half2(0.5, 0.5),
					half2(_Scale, _Scale)
				);
				fixed4 color = tex2D(_SelfTexture2D, uv);
				color *= 1 - _Dampen;
				
				color += dot;
				color = BACKGROUND_BY_RANGE(color, 0.2, 0.8);
				color += highlight * 0.5;
				
				return color;
			}
			ENDCG
		}
	}
}