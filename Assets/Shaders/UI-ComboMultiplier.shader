Shader "UI/ComboMultiplier"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_Power ("Power", Float) = 1
		_MinRange ("Min Range", Range(0, 1)) = 0
		_MaxRange ("Max Range", Range(0, 1)) = 1
		
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		[Toggle(BACKGROUND_SCHEME)] _UseBackgroundScheme ("Use Background Scheme", Float) = 0
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
		Blend SrcAlpha One
		ColorMask [_ColorMask]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Math.cginc"
			#include "UIMask.cginc"
			#include "Color.cginc"
			#include "UnityCG.cginc"

			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP
			#pragma multi_compile_local _ BACKGROUND_SCHEME

			struct vertData
			{
				float4 vertex : POSITION;
				float4 color  : COLOR;
				float2 uv     : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct fragData
			{
				float4 vertex : SV_POSITION;
				fixed4 color  : COLOR;
				float2 uv     : TEXCOORD0;
				half4  mask   : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			fixed4 _TextureSampleAdd;
			fixed4 _Color;
			float _Power;
			fixed _MinRange;
			fixed _MaxRange;
			float _Spectrum[64];

			fragData vert(const vertData IN)
			{
				fragData OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				fixed offset = frac(_Time.y);
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv + half2(offset, offset * 2);
				OUT.mask = getUIMask(OUT.vertex.w, IN.vertex.xy);
				OUT.color = IN.color * _Color;
				return OUT;
			}

			fixed4 frag(const fragData IN) : SV_Target
			{
				half4 color = IN.color * (tex2D(_MainTex, IN.uv) + _TextureSampleAdd);
				
				half spectrum = 0;
				for (int i = 0; i < 5; i++)
					spectrum += _Spectrum[16 + i];
				spectrum *= 0.2;
				spectrum = remap01(spectrum, _MinRange,_MaxRange);
				
				#ifdef BACKGROUND_SCHEME
				color.rgb *= lerp(_BackgroundSecondaryColor, _BackgroundPrimaryColor, spectrum);
				#else
				color.rgb *= lerp(_ForegroundSecondaryColor, _ForegroundPrimaryColor, spectrum);
				#endif
				
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
