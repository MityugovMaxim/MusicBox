Shader "UI/Fluid"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags
		{ 
			"Queue" = "Geometry" 
			"IgnoreProjector" = "True" 
			"RenderType" = "Opaque" 
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Cull Back
		Lighting Off
		ZWrite On
		Fog { Mode Off }
		Blend Off

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Color.cginc"
			
			struct vertData
			{
				float4 vertex : POSITION;
				float4 color  : COLOR;
				float2 uv     : TEXCOORD0;
			};

			struct fragData
			{
				float4 vertex : SV_POSITION;
				fixed4 color  : COLOR;
				half2 uv      : TEXCOORD0;
			};

			fixed4 _Color;
			sampler2D _MainTex;

			fragData vert(const vertData IN)
			{
				fragData OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv;
				OUT.color = IN.color * _Color;
				return OUT;
			}

			fixed4 frag(const fragData IN) : SV_Target
			{
				const fixed value = tex2D(_MainTex, IN.uv).r;
				
				const fixed highlight = smoothstep(0.9, 1, value) * 0.15;
				
				fixed4 color = BACKGROUND_BY_RANGE(value, 0.3, 0.95) * value * 0.7;
				
				color.rgb += highlight;
				
				return IN.color * _BackgroundSecondaryColor + color;
			}
		ENDCG
		}
	}
}
