float rand(float2 value)
{ 
    return frac(sin(dot(value, float2(12.9898, 4.1414))) * 43758.5453);
}

float noise(float2 value)
{
    float2 a = floor(value);
    float2 b = frac(value);
    
    b = b * b * (3.0 - 2.0 * b);
    
    float result = lerp(
        lerp(rand(a), rand(a + float2(1, 0)), b.x),
        lerp(rand(a + float2(0, 1)), rand(a + float2(1, 1)), b.x),
        b.y
    );
    
    return sin(result * result * (_Time.y * 10)) * 0.5 + 0.5;
}

float remap(float value, float l1, float h1, float l2, float h2)
{
    return l2 + (value - l1) * (h2 - l2) / (h1 - l1);
}

float circle(half2 _Position, float _Radius, float _Smooth)
{
    float value = length(_Position);
    return smoothstep(_Radius, _Radius - _Smooth, value);
}

float ring(half2 _Position, float _OutRadius, float _InRadius, float _Smooth)
{
    float value     = length(_Position);
    float outCircle = smoothstep(_OutRadius, _OutRadius - _Smooth, value);
    float inCircle  = smoothstep(_InRadius, _InRadius + _Smooth, value);
    return outCircle * inCircle;
}

float getRadians(float _Angle)
{
    return _Angle * 0.017453292;
}

float getAngle(float _Radians)
{
    return _Radians * 57.295779513;
}

half2 rotate(half2 _Vector, half2 _Pivot, float _Angle)
{
    float angle = getRadians(_Angle);
    
    _Vector -= _Pivot;
    _Vector = half2(
        _Vector.x * angle + _Vector.y * -angle,
        _Vector.x * angle + _Vector.y * angle
    );
    _Vector += _Pivot;
    
    return _Vector;
}