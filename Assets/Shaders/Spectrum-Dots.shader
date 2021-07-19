Shader "Spectrum/Dots"
{
	Properties
	{
		_Scale ("Scale", Float) = 0.1
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
			#include "Color.cginc"
			#include "UnityCustomRenderTexture.cginc"
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment frag

			float _Spectrum[64];
			fixed _Scale;
			fixed _Dampen;

			float4 frag(const v2f_customrendertexture IN) : COLOR
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
				
				color.a *= 1 - _Dampen;
				
				color = clamp(color + value, 0, 1);
				
				color = BACKGROUND_BY_ALPHA(color);
				
				return color;
			}
			ENDCG
		}
	}
}