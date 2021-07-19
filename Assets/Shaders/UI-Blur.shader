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
				float2 uv     : TEXCOORD0;
			};

			struct fragData
			{
				float4 vertex : SV_POSITION;
				float2 uv     : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			fragData vert (vertData IN)
			{
				fragData OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv;
				return OUT;
			}

			fixed4 frag (fragData IN) : SV_Target
			{
				const half x = _MainTex_TexelSize.x;
				const half y = _MainTex_TexelSize.y;
				
				fixed4 color = tex2D(_MainTex, IN.uv);
				
				color.rgb += tex2D(_MainTex, IN.uv + half2(x, y)).rgb;
				color.rgb += tex2D(_MainTex, IN.uv + half2(x, 0)).rgb;
				color.rgb += tex2D(_MainTex, IN.uv + half2(x, -y)).rgb;
				
				color.rgb += tex2D(_MainTex, IN.uv + half2(0, y)).rgb;
				color.rgb += tex2D(_MainTex, IN.uv + half2(0, -y)).rgb;
				
				color.rgb += tex2D(_MainTex, IN.uv + half2(-x, y)).rgb;
				color.rgb += tex2D(_MainTex, IN.uv + half2(-x, 0)).rgb;
				color.rgb += tex2D(_MainTex, IN.uv + half2(-x, -y)).rgb;
				
				color.rgb /= 9;
				
				color.a = 1;
				
				return color;
			}
			ENDCG
		}
	}
}
