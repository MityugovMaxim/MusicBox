Shader "Elements/Chamfer"
{
	Properties
	{
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		[HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
		[HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
		[HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
		[HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
		[HideInInspector] _ColorMask ("Color Mask", Float) = 15
		[HideInInspector] _BlendSrc ("Blend Src", Int) = 0
		[HideInInspector] _BlendDst ("Blend Dst", Int) = 0
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
		Blend [_BlendSrc] [_BlendDst]
		ColorMask [_ColorMask]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Assets/Shaders/Math.cginc"

			struct vertData
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
				float2 mask : TEXCOORD1;
				float4 rect : TEXCOORD2;
				float4 radius : TEXCOORD3;
			};

			struct fragData
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
				float4 rect : TEXCOORD1;
				float2 position : TEXCOORD2;
				float4 radius : TEXCOORD3;
			};

			sampler2D _MainTex;

			fragData vert (const vertData IN)
			{
				fragData OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv;
				OUT.color = IN.color;
				OUT.rect = IN.rect;
				OUT.position = IN.rect.xy + IN.rect.zw * IN.mask;
				OUT.radius = IN.radius;
				return OUT;
			}

			float ClipLeft(float2 _Position, float4 _Rect, float _Value)
			{
				return step(_Position.x, _Rect.x + _Value);
			}

			float ClipRight(float2 _Position, float4 _Rect, float _Value)
			{
				return step(_Rect.x + _Rect.z - _Value, _Position.x);
			}

			float ClipTop(float2 _Position, float4 _Rect, float _Value)
			{
				return step(_Rect.y + _Rect.w - _Value, _Position.y);
			}

			float ClipBottom(float2 _Position, float4 _Rect, float _Value)
			{
				return step(_Position.y, _Rect.y + _Value);
			}

			fixed4 frag (const fragData IN) : SV_Target
			{
				const float smooth = 0.05;
				
				const fixed4 color = tex2D(_MainTex, IN.uv);
				
				const float2 position = IN.rect.xy - IN.position;
				
				float tl = IN.radius.x;
				float tr = IN.radius.y;
				float bl = IN.radius.z;
				float br = IN.radius.w;
				
				// Top Left
				const float  tlClip = ClipLeft(IN.position.x, IN.rect, tl) * ClipTop(IN.position, IN.rect, tl);
				const float2 tlOffset = float2(tl, IN.rect.w - tl);
				const float  tlPhase = getCircle(position + tlOffset, tl, smooth) * tlClip;
				
				// Top Right
				const float  trClip = ClipRight(IN.position, IN.rect, tr) * ClipTop(IN.position, IN.rect, tr);
				const float2 trOffset = float2(IN.rect.z - tr, IN.rect.w - tr);
				const float  trPhase = getCircle(position + trOffset, tr, smooth) * trClip;
				
				// Bottom Left
				const float  blClip = ClipLeft(IN.position, IN.rect, bl) * ClipBottom(IN.position, IN.rect, bl);
				const float2 blOffset = float2(bl, bl);
				const float  blPhase = getCircle(position + blOffset, bl, smooth) * blClip;
				
				// Bottom Right
				const float  brClip = ClipRight(IN.position, IN.rect, br) * ClipBottom(IN.position, IN.rect, br);
				const float2 brOffset = float2(IN.rect.z - br, br);
				const float  brPhase = getCircle(position + brOffset, br, smooth) * brClip;
				
				const float body = 1 - tlClip - trClip - blClip - brClip;
				
				const float alpha = body + tlPhase + trPhase + blPhase + brPhase;
				
				const fixed4 mask = fixed4(1, 1, 1, alpha);
				
				return color * IN.color * mask;
			}
			ENDCG
		}
	}
}
