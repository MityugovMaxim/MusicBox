Shader "Spectrum/Boids"
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

			float4 frag(v2f_customrendertexture IN) : COLOR
			{
				const half aspect = _ScreenParams.x / _ScreenParams.y;
				half2 uv = IN.localTexcoord;
				uv -= 0.5;
				uv /= 0.5;
				uv.y /= aspect;
				
				const half size = 63;
				
				const half step = UNITY_TWO_PI / size;
				
				half value = 0;
				for (int i = 0; i < size; i++)
				{
					half angle = step * i + _Time.w;
					half2x2 rotation =
					{
						cos(angle), -sin(angle),
						-sin(angle), cos(angle)
					};
					
					half2 position = half2(0.45 + _Spectrum[i] * 0.5, 0);
					
					position = mul(position, rotation);
					
					value += smoothstep(0.06, 0.03, length(position - uv));
				}
				
				half2 coord = IN.localTexcoord;
				half2x2 scale =
				{
					0.95, 0,
					0, 0.95
				};
				coord -= 0.5;
				coord = mul(coord, scale);
				coord += 0.5;
				
				fixed4 color = tex2D(_SelfTexture2D, coord);
				
				const float average = (color.r + color.g + color.b) * 0.333333;
				color *= lerp(fixed4(0.32, 0.64, 0.72, 0.8), fixed4(0.72, 0.10, 0.23, 0.8), average);
				
				value *= value;
				fixed4 bend = lerp(_SourceColor, _TargetColor, value);
				bend.a = value;
				
				color += bend;
				
				return color;
			}
			ENDCG
		}
	}
}