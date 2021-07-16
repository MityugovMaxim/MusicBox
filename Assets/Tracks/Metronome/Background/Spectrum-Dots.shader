Shader "Spectrum/Dots"
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
			#include "UnityCustomRenderTexture.cginc"
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment frag

			float _Spectrum[64];
			fixed4 _SourceColor;
			fixed4 _TargetColor;
			fixed4 _TraceColor;
			fixed _Scale;

			half remap01(float _Value, float _Low, float _High)
			{
				return (_Value - _Low) / (_High - _Low);
			}

			float4 frag(v2f_customrendertexture IN) : COLOR
			{
				const half aspect = _ScreenParams.x / _ScreenParams.y;
				half2 uv = IN.localTexcoord;
				uv.y /= aspect;
				
				const half size = 63;
				
				const half step = 1.0 / size;
				const int index = floor(uv.x * size);
				
				const half spectrum = _Spectrum[index];
				
				const half2 position = half2(step * 0.5 + step * index, spectrum * 0.5 + 0.45 / aspect);
				
				const fixed value = smoothstep(0.5 / size, 0.5 / (size + size), length(position - uv));
				
				fixed4 color = tex2D(_SelfTexture2D, IN.localTexcoord);
				
				const float average = (color.r + color.g + color.b) * 0.333333;
				color *= lerp(fixed4(0.32, 0.64, 0.72, 0.8), fixed4(0.62, 0.70, 0.72, 0.8), average);
				
				fixed4 bend = lerp(_SourceColor, _TargetColor, value);
				bend.a = value;
				
				color += bend;
				
				return color;
			}
			ENDCG
		}
	}
}