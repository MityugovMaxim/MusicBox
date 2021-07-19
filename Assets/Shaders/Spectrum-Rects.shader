Shader "Spectrum/Rects"
{
	Properties { }

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

			float4 frag(const v2f_customrendertexture IN) : COLOR
			{
				const half size = 63;
				
				const int index = floor(IN.localTexcoord.x * size);
				
				const half width = frac(IN.localTexcoord.x * size);
				const half gap = step(0.075, width) * step(width, 0.925);
				
				const half spectrum = _Spectrum[index] * 0.25;
				
				const fixed value = smoothstep(spectrum, 0, IN.localTexcoord.y) * gap;
				
				fixed4 color = BACKGROUND_BY_PHASE(1 - value);
				color.a *= 1 - step(value, 0);
				
				return color;
			}
			ENDCG
		}
	}
}