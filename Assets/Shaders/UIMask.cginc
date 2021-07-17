float4 _ClipRect;
float  _UIMaskSoftnessX;
float  _UIMaskSoftnessY;

half4 getUIMask(const float2 _Position, const float _ScaleFactor)
{
	const float2 pixelSize = _ScaleFactor / float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
	const float4 rect = clamp(_ClipRect, -2e10, 2e10);
	
	return half4(
		_Position * 2 - rect.xy - rect.zw,
		0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy))
	);
}

fixed4 useUIMask(fixed4 _Color, const half4 _Mask)
{
	#ifdef UNITY_UI_CLIP_RECT
	const half2 mask = saturate((_ClipRect.zw - _ClipRect.xy - abs(_Mask.xy)) * _Mask.zw);
	_Color.a *= mask.x * mask.y;
	#endif
	
	#ifdef UNITY_UI_ALPHACLIP
	clip (_Color.a - 0.001);
	#endif
	
	return _Color;
}