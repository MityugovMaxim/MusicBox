Shader "UI/Blur"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		Blend Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct vertData
			{
				float4 vertex : POSITION;
				half2 uv      : TEXCOORD0;
			};

			struct fragData
			{
				float4 vertex : SV_POSITION;
				half2 uv      : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			fragData vert (const vertData IN)
			{
				fragData OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv;
				return OUT;
			}

			fixed4 frag (const fragData IN) : SV_Target
			{
				const half x = _MainTex_TexelSize.x;
				const half y = _MainTex_TexelSize.y;
				
				// kernel [ 1 2 3 3 3 2 1 ] : 15
				
				// horizontal
				fixed3 horizontal = fixed3(0, 0, 0);
				horizontal += tex2D(_MainTex, IN.uv + half2(-x * 3, 0)).rgb;
				horizontal += tex2D(_MainTex, IN.uv + half2(-x * 2, 0)).rgb * 2;
				horizontal += tex2D(_MainTex, IN.uv + half2(-x, 0)).rgb * 3;
				horizontal += tex2D(_MainTex, IN.uv).rgb * 3;
				horizontal += tex2D(_MainTex, IN.uv + half2(x, 0)).rgb * 3;
				horizontal += tex2D(_MainTex, IN.uv + half2(x * 2, 0)).rgb * 2;
				horizontal += tex2D(_MainTex, IN.uv + half2(x * 3, 0)).rgb;
				horizontal /= 15;
				
				// vertical
				fixed3 vertical = fixed3(0, 0, 0);
				vertical += tex2D(_MainTex, IN.uv + half2(0, -y * 3)).rgb;
				vertical += tex2D(_MainTex, IN.uv + half2(0, -y * 2)).rgb * 2;
				vertical += tex2D(_MainTex, IN.uv + half2(0, -y)).rgb * 3;
				vertical += tex2D(_MainTex, IN.uv).rgb * 3;
				vertical += tex2D(_MainTex, IN.uv + half2(0, y)).rgb * 3;
				vertical += tex2D(_MainTex, IN.uv + half2(0, y * 2)).rgb * 2;
				vertical += tex2D(_MainTex, IN.uv + half2(0, y * 3)).rgb;
				vertical /= 15;
				
				fixed4 color = fixed4((horizontal + vertical) * 0.5, 1);
				
				const fixed grayscale = 0.299 * color.r + 0.587 * color.g + 0.114 * color.b;
				
				color.rgb = lerp(color.rgb, grayscale, 0.1);
				
				return color;
			}
			ENDCG
		}
	}
}
