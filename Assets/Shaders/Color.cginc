fixed4 _BackgroundPrimaryColor;
fixed4 _BackgroundSecondaryColor;
fixed4 _ForegroundPrimaryColor;
fixed4 _ForegroundSecondaryColor;

#define BACKGROUND_BY_RANGE(_Color, _Min, _Max) colorByRange(_Color, _Min, _Max, _BackgroundSecondaryColor, _BackgroundPrimaryColor)
#define BACKGROUND_BY_ALPHA(_Color) colorByAlpha(_Color, _BackgroundSecondaryColor, _BackgroundPrimaryColor)
#define BACKGROUND_BY_GRAYSCALE(_Color) colorByGrayscale(_Color, _BackgroundSecondaryColor, _BackgroundPrimaryColor)
#define BACKGROUND_BY_LUMINANCE(_Color) colorByLuminance(_Color, _BackgroundSecondaryColor, _BackgroundPrimaryColor)
#define BACKGROUND_BY_PHASE(_Phase) lerp(_BackgroundSecondaryColor, _BackgroundPrimaryColor, _Phase)

#define FOREGROUND_BY_RANGE(_Color, _Min, _Max) colorByRange(_Color, _Min, _Max, _ForegroundSecondaryColor, _ForegroundPrimaryColor)
#define FOREGROUND_BY_ALPHA(_Color) colorByAlpha(_Color, _ForegroundSecondaryColor, _ForegroundPrimaryColor)
#define FOREGROUND_BY_GRAYSCALE(_Color) colorByGrayscale(_Color, _ForegroundSecondaryColor, _ForegroundPrimaryColor)
#define FOREGROUND_BY_LUMINANCE(_Color) colorByLuminance(_Color, _ForegroundSecondaryColor, _ForegroundPrimaryColor)
#define FOREGROUND_BY_PHASE(_Phase) lerp(_ForegroundSecondaryColor, _ForegroundPrimaryColor, _Phase) 

fixed4 colorByRange(const fixed4 _Color, const fixed _Min, const fixed _Max, const fixed4 _Source, const fixed4 _Target)
{
	const fixed phase = (_Color.a - _Min) / (_Max - _Min);
	fixed4 color = lerp(_Source, _Target, phase);
	color.a *= _Color.a;
	return color;
}

fixed4 colorByAlpha(const fixed4 _Color, const fixed4 _Source, const fixed4 _Target)
{
	fixed4 color = lerp(_Source, _Target, _Color.a);
	color.a *= _Color.a;
	return color;
}

fixed4 colorByGrayscale(const fixed4 _Color, const fixed4 _Source, const fixed4 _Target)
{
	const fixed grayscale = (_Color.r + _Color.g + _Color.b) * 0.333333;
	fixed4 color = lerp(_Source, _Target, grayscale);
	color.a *= _Color.a;
	return color;
}

fixed4 colorByLuminance(const fixed4 _Color, const fixed4 _Source, const fixed4 _Target)
{
	const fixed luminance = _Color.r * 0.299 + _Color.g * 0.587 + _Color.b * 0.114;
	fixed4 color = lerp(_Source, _Target, luminance);
	color.a *= _Color.a;
	return color;
}