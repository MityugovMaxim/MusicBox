Shader "UI/Rays"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_FogTex ("Fog Tex", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_Speed ("Speed", Float) = 1
		_Offset ("Offset", Float) = 1
		
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		[HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
		[HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
		[HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
		[HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
		[HideInInspector] _ColorMask ("Color Mask", Float) = 15
	}

	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend DstColor Zero
		ColorMask [_ColorMask]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UIMask.cginc"
			#include "Math.cginc"
			#include "UnityCG.cginc"
			#include "Color.cginc"

			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

			struct vertData
			{
				float4 vertex : POSITION;
				fixed4 color  : COLOR;
				half2  uv     : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct fragData
			{
				float4 vertex : SV_POSITION;
				fixed4 color  : COLOR;
				float2 uv     : TEXCOORD0;
				half4  mask   : TEXCOORD1;
				half4  fog1   : TEXCOORD2;
				half4  fog2   : TEXCOORD3;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			sampler2D _FogTex;
			half _Speed;
			half _Offset;
			fixed4 _TextureSampleAdd;
			fixed4 _Color;

			fragData vert(const vertData IN)
			{
				fragData OUT;
				
				const fixed offset1 = frac(_Time.x * _Speed);
				const fixed offset2 = frac((_Time.x + _Offset) * _Speed);
				
				const fixed value1 = smoothstep(0, 0.4, offset1) * smoothstep(1, 0.5, offset1);
				const fixed value2 = smoothstep(0, 0.4, offset2) * smoothstep(1, 0.5, offset2);
				
				UNITY_SETUP_INSTANCE_ID(IN);
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv;
				OUT.mask = getUIMask(OUT.vertex.w, IN.vertex.xy);
				OUT.fog1 = half4((IN.uv - 0.5) * (1 - offset1) + 0.5, value1, value1);
				OUT.fog2 = half4((IN.uv - 0.5) * (1 - offset2) + 0.5, value2, value2);
				OUT.color = IN.color * _Color;
				return OUT;
			}

			fixed4 frag(const fragData IN) : SV_Target
			{
				const half2 offset = sin(IN.uv.yx * 10 + _Time.y * 5) * half2(0.005, 0.005);
				const fixed4 fog1 = tex2D(_FogTex, IN.fog1 + offset) * IN.fog1.z;
				const fixed4 fog2 = tex2D(_FogTex, IN.fog2 + offset) * IN.fog2.z;
				
				fixed4 color = IN.color * (tex2D(_MainTex, IN.uv) + _TextureSampleAdd);
				
				color.rgb += (fog1 + fog2) * (color.rgb * color.rgb) * 20 * grayscale(color);
				color.rgb += 1 - color.a;
				
				#ifdef UNITY_UI_CLIP_RECT
				half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
				color.a *= m.x * m.y;
				#endif
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif
				
				return color;
			}
			ENDCG
		}
	}
}
