Shader "Sprite/Default"
{
	Properties
	{
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct vertData
			{
				float4 vertex : POSITION;
				float4 color  : COLOR;
				half2  uv     : TEXCOORD0;
			};

			struct fragData
			{
				float4 vertex : SV_POSITION;
				fixed4 color  : COLOR;
				half2  uv     : TEXCOORD0;
			};

			sampler2D _MainTex;
			fixed4 _TextureSampleAdd;
			fixed4 _Color;

			fragData vert(const vertData IN)
			{
				fragData OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.color = IN.color * _Color;
				OUT.uv = IN.uv;
				return OUT;
			}

			fixed4 frag(const fragData IN) : SV_Target
			{
				fixed4 color = IN.color * (tex2D(_MainTex, IN.uv) + _TextureSampleAdd);
				
				return color;
			}
			ENDCG
		}
	}
}
