Shader "UI/Spectrum"
{
	Properties
	{
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		[Toggle(COLOR_SCHEME)] _ColorScheme ("Color Scheme", Float) = 0
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
			#pragma multi_compile_local _ COLOR_SCHEME

			#include "UnityCG.cginc"
			#include "Color.cginc"

			struct vertData
			{
				float4 vertex : POSITION;
				float4 color  : COLOR;
				half2  uv     : TEXCOORD0;
				half4  offset : TEXCOORD1;
			};

			struct fragData
			{
				float4 vertex : SV_POSITION;
				fixed4 color  : COLOR;
				half2  uv     : TEXCOORD0;
			};

			sampler2D _MainTex;
			float _Spectrum[64];
			fixed4 _Color;

			fragData vert(const vertData IN)
			{
				const float2 offset = _Spectrum[IN.offset.z] * IN.offset.xy * IN.offset.w;
				
				fragData OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex + float4(offset.xy, 0, 0));
				OUT.color = IN.color * _Color;
				OUT.uv = IN.uv;
				return OUT;
			}

			fixed4 frag(const fragData IN) : SV_Target
			{
				fixed4 color = tex2D(_MainTex, IN.uv) * IN.color;
				
				#ifdef COLOR_SCHEME
				color.rgb *= _BackgroundSecondaryColor;
				#endif
				
				return color;
			}
			ENDCG
		}
	}
}
