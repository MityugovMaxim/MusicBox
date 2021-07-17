float rand(const half2 _Value)
{ 
	return frac(sin(dot(_Value, half2(12.9898, 4.1414))) * 43758.5453);
}

float noise(const half2 _Value)
{
	half2 a = floor(_Value);
	half2 b = frac(_Value);
	
	b = b * b * (3.0 - 2.0 * b);
	
	const half result = lerp(
		lerp(rand(a), rand(a + half2(1, 0)), b.x),
		lerp(rand(a + half2(0, 1)), rand(a + half2(1, 1)), b.x),
		b.y
	);
	
	return sin(result * result * (_Time.y * 10)) * 0.5 + 0.5;
}

half remap01(const half _Value, const half _Low, const half _High)
{
	return (_Value - _Low) / (_High - _Low);
}

half remap(const half _Value, const half _Low1, const half _High1, const half _Low2, const half _High2)
{
	return _Low2 + (_Value - _Low1) * (_High2 - _Low2) / (_High1 - _Low1);
}

fixed grayscale(const fixed3 _Color)
{
	return _Color.r * 0.299 + _Color.g * 0.587 + _Color.b * 0.114;
}

half getCircle(const half2 _Position, const half _Radius, const half _Smooth)
{
	return smoothstep(_Radius, _Radius - _Smooth, length(_Position));
}

half getRing(const half2 _Position, const half _OutRadius, const half _InRadius, const half _Smooth)
{
	const half value     = length(_Position);
	const half outCircle = smoothstep(_OutRadius, _OutRadius - _Smooth, value);
	const half inCircle  = smoothstep(_InRadius, _InRadius + _Smooth, value);
	return outCircle * inCircle;
}

half getLine(const half2 _Position, const half2 _A, const half2 _B, const half _Width, const half _Smooth)
{
	const half2 a = _Position - _A;
	const half2 b = _B - _A;
	const half phase = clamp(dot(a, b) / dot(b, b), 0, 1);
	const half value = length(a - b * phase);
	return smoothstep(_Width, _Width - _Smooth, length(a - b * phase));
}

half getRadians(const half _Angle)
{
	return _Angle * 0.017453292;
}

half getAngle(const half _Radians)
{
	return _Radians * 57.295779513;
}

half2 scale(const half2 _Vector, const half _Scale)
{
	const half2x2 scale = { _Scale, 0, 0, _Scale };
	return mul(_Vector, scale);
}

half2 scale(half2 _Vector, const half2 _Pivot, const half2 _Scale)
{
	const half2x2 scale = { _Scale.x, 0, 0, _Scale.y };
	_Vector -= _Pivot;
	_Vector = mul(_Vector, scale);
	_Vector += _Pivot;
	return _Vector;
}

half2 rotate(const half2 _Vector, const half _Angle)
{
	const half s = sin(_Angle);
	const half c = cos(_Angle); 
	const half2x2 rotation = { c, -s, -s, c };
	return mul(_Vector, rotation);
}

half2 rotate(half2 _Vector, const half2 _Pivot, const half _Angle)
{
	const half s = sin(_Angle);
	const half c = cos(_Angle);
	const half2x2 rotation = { c, -s, -s, c };
	_Vector -= _Pivot;
	_Vector = mul(_Vector, rotation);
	_Vector += _Pivot;
	return _Vector;
}

half2 rotate45(half2 _Vector, const half2 _Pivot)
{
	const half value = 0.7071067812;
	_Vector -= _Pivot;
	_Vector = half2(
		_Vector.x * value + _Vector.y * -value,
		_Vector.x * value + _Vector.y * value
	);
	_Vector += _Pivot;
	return _Vector;
}

half2 rotate90(half2 _Vector, const half2 _Pivot)
{
	_Vector -= _Pivot;
	_Vector = half2(-_Vector.y, _Vector.x);
	_Vector += _Pivot;
	return _Vector;
}

half2 rotate180(half2 _Vector, const half2 _Pivot)
{
	_Vector -= _Pivot;
	_Vector = half2(-_Vector.x, -_Vector.y);
	_Vector += _Pivot;
	return _Vector;
}

half2 rotate270(half2 _Vector, const half2 _Pivot)
{
	_Vector -= _Pivot;
	_Vector = half2(_Vector.y, -_Vector.x);
	_Vector += _Pivot;
	return _Vector;
}