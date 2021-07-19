Shader "Spectrum/Boids"
{
	Properties
	{
		_Scale ("Scale", Float) = 0.9
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
			fixed _Scale;
			fixed _Dampen;

			float4 frag(v2f_customrendertexture IN) : COLOR
			{
				const half aspect = _ScreenParams.x / _ScreenParams.y;
				half2 base = IN.localTexcoord;
				base -= 0.5;
				base /= 0.5;
				base.y /= aspect;
				
				const half size = 63;
				
				const half step = UNITY_TWO_PI / size;
				
				half value = 0;
				for (int i = 0; i < size; i++)
				{
					const half angle = step * i + _Time.w;
					
					half2 position = half2(0.45 + _Spectrum[i] * 0.5, 0);
					
					position = rotate(position, angle);
					
					value += smoothstep(0.06, 0.03, length(position - base));
				}
				value *= value;
				
				const half2 uv = scale(IN.localTexcoord, half2(0.5, 0.5), half2(_Scale, _Scale));
				
				fixed4 color = tex2D(_SelfTexture2D, uv);
				color.a *= 1 - _Dampen;
				
				color = clamp(color + value, 0, 1);
				
				color = BACKGROUND_BY_ALPHA(color);
				
				return color;
			}
			ENDCG
		}
	}
}