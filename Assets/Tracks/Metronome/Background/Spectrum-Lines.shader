Shader "Spectrum/Lines"
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

			half getLineDistance(half2 position, half2 a, half2 b)
			{
				half2 pa = position - a;
				half2 ba = b - a;
				float phase = clamp(dot(pa, ba) / dot(ba, ba), 0, 1);
				return length(pa - ba * phase);
			}

			float4 frag(v2f_customrendertexture IN) : COLOR
			{
				const half aspect = _ScreenParams.x / _ScreenParams.y;
				half2 uv = IN.localTexcoord;
				uv.y /= aspect;
				
				const half size = 63;
				
				const half step = 1.0 / size;
				const int index = floor(uv.x * size);
				const int prev = max(0, index - 1);
				const int next = min(index + 1, size - 1);
				
				const half spectrum = _Spectrum[index];
				const half nextSpec = _Spectrum[next];
				const half prevSpec = _Spectrum[prev];
				
				const half2 position = half2(step * 0.5 + step * index, spectrum * 0.5 + 0.35 / aspect);
				const half2 nextPos = half2(step * 0.5 + step * next, nextSpec * 0.5 + 0.35 / aspect);
				const half2 prevPos = half2(step * 0.5 + step * prev, prevSpec * 0.5 + 0.35 / aspect);
				
				fixed value = 0;
				value += smoothstep(0.008, 0, getLineDistance(uv, position, prevPos));
				value += smoothstep(0.008, 0, getLineDistance(uv, position, nextPos));
				
				const half2 scale = half2(0.98, 0.95);
				half2 coord = IN.localTexcoord;
				coord -= 0.5;
				coord = mul(coord, half2x2(scale.x, 0, 0, scale.y));
				coord += 0.5;
				//coord.y += 0.01;
				fixed4 color = tex2D(_SelfTexture2D, coord);
				
				const float average = (color.r + color.g + color.b) * 0.333333;
				color *= lerp(fixed4(0.32, 0.64, 0.72, 0.8), fixed4(0.72, 0.10, 0.23, 0.8), average);
				
				fixed4 bend = lerp(_SourceColor, _TargetColor, value);
				bend.a = value;
				
				color += bend;
				
				return color;
			}
			ENDCG
		}
	}
}