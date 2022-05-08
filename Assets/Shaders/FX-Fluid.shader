Shader "FX/Fluid"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" { }
		_NormalTex ("Normal", 2D) = "black" { }
		_Scale ("Scale", Range(0, 2)) = 1
		_Refraction ("Refraction", Range(0, 1)) = 0.1
		_Dampen ("Dampen", Range(0, 1)) = 0
		_Amplitude ("Amplitude", Range(0, 1)) = 0.05
		_Speed ("Speed", Float) = 1
		_Frequency ("Frequency", Float) = 10
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"RenderQueue" = "Transparent"
		}
		
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Math.cginc"

			struct vertData
			{
				float4 vertex : POSITION;
				half2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct fragData
			{
				float4 vertex : SV_POSITION;
				half2 uv : TEXCOORD0;
				fixed4 color : COLOR;
				half2 mask : TEXCOORD1;
				half4 wave : TEXCOORD2;
			};

			sampler2D _MainTex;
			sampler2D _NormalTex;
			float _Scale;
			float _Refraction;
			float _Dampen;
			float _Amplitude;
			float _Speed;
			float _Frequency;

			fixed pong(const fixed _Value)
			{
				const fixed length = 1;
				const fixed time = frac(_Value) * 2;
				return length - abs(time - length);
			}

			float GetAspect()
			{
				return _ScreenParams.x / _ScreenParams.y;
			}

			fragData vert(const vertData IN)
			{
				fragData OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv;
				OUT.uv -= 0.5;
				OUT.uv *= _Scale;
				OUT.uv += 0.5;
				
				const float aspect = GetAspect();
				OUT.mask = IN.uv;
				OUT.mask.x -= 0.5;
				OUT.mask.x *= aspect;
				OUT.mask.x += 0.5;
				
				OUT.color = IN.color;
				
				OUT.wave.xy = half2(rotate(_Amplitude, _Time.y * _Speed));
				OUT.wave.zw = half2(rotate(_Amplitude, _Time.y * _Speed * 0.5));
				
				return OUT;
			}

			fixed4 frag(const fragData IN) : SV_Target
			{
				const half2 wave1 = sin(IN.mask.xy * _Frequency + _Time.y) * IN.wave.xy;
				const half2 wave2 = sin(IN.mask.xy * _Frequency * 0.5 + _Time.x) * IN.wave.zw;
				
				const fixed4 normal = tex2D(_NormalTex, IN.mask.xy + wave1 + wave2);
				
				const fixed3 offset = UnpackNormal(normal) * _Refraction;
				
				const half2 uv = IN.uv - offset;
				
				return tex2D(_MainTex, uv) * (1 - _Dampen) * IN.color;
			}
			ENDCG
		}
	}
}