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

			float4 frag(const v2f_customrendertexture IN) : COLOR
			{
				half2 base = IN.localTexcoord * 2 - 1;
				base.y /= _ScreenParams.x / _ScreenParams.y;
				
				const half size = 61;
				
				fixed distances[size];
				
				const half step = UNITY_TWO_PI / size;
				const half s = sin(step);
				const half c = cos(step);
				half2 normal = rotate(half2(1, 0), -_Time.w);
				for (int i = 0; i < size; i++)
				{
					const half2 position = normal * (0.45 + _Spectrum[i] * 0.5) - base;
					
					distances[i] = position.x * position.x + position.y * position.y;
					
					normal = rotate(normal, s, c);
				}
				
				const half2 uv = scale(
					IN.localTexcoord,
					half2(0.5, 0.5),
					half2(_Scale, _Scale)
				);
				
				fixed4 color = tex2D(_SelfTexture2D, uv);
				color.a *= 1 - _Dampen;
				
				fixed value     = 0;
				fixed highlight = 0;
				for (int i = 0; i < size; i++)
				{
					const fixed valueMin = 0.0004; // 0.02
					const fixed valueMax = 0.0064;  // 0.08
					value += smoothstep(valueMax, valueMin, distances[i]);
					
					const fixed highlightMin = 0.000001; // 0.001
					const fixed highlightMax = 0.0025;    // 0.05
					highlight += smoothstep(highlightMax, highlightMin, distances[i]);
				}
				value *= value;
				
				color += value * 0.1;
				color = BACKGROUND_BY_RANGE(color, 0.2, 0.8);
				color += highlight * 0.5;
				
				return color;
			}
			ENDCG
		}
	}
}