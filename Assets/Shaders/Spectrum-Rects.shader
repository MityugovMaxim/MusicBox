Shader "Spectrum/Rects"
{
	Properties
	{
		_SourceColor ("Source", Color) = (1,1,1,1)
		_TargetColor ("Target", Color) = (1,1,1,1)
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

			float4 frag(v2f_customrendertexture IN) : COLOR
			{
				const half size = 63;
				
				const int index = floor(IN.localTexcoord.x * size);
				
				const half width = frac(IN.localTexcoord.x * size);
				const half gap = step(0.05, width) * step(width, 0.95);
				
				const half spectrum = _Spectrum[index] * 0.25;
				
				const fixed value = smoothstep(spectrum, 0, IN.localTexcoord.y) * gap;
				
				fixed4 color = lerp(_SourceColor, _TargetColor, value);
				color.a *= 1 - step(value, 0);
				
				return color;
			}
			ENDCG
		}
	}
}