using System;
using System.Collections.Generic;
using UnityEngine;

public enum ScaleMode
{
	Stretch = 0,
	Fit     = 1,
	Fill    = 2,
	Crop    = 3,
}

public enum BorderMode
{
	Stretch,
	Fit,
	Fill,
}

public static class MeshUtility
{
	public static void GenerateMesh(
		Rect          _Rect,
		Sprite        _Sprite,
		ScaleMode     _ScaleMode,
		BorderMode    _BorderMode,
		float         _BorderScale,
		TextAnchor    _Alignment,
		Vector4       _Border,
		List<float>   _HorizontalVertex,
		List<float>   _VerticalVertex,
		List<float>   _HorizontalUV,
		List<float>   _VerticalUV,
		List<Vector3> _Vertices,
		List<Vector2> _UV,
		List<int>     _Triangles,
		bool          _FillCenter = true 
	)
	{
		Rect sprite = _Sprite != null ? _Sprite.textureRect : _Rect;
		
		int width  = _Sprite != null ? _Sprite.texture.width : (int)_Rect.width;
		int height = _Sprite != null ? _Sprite.texture.height : (int)_Rect.height;
		
		Vector2 pivot = _Alignment.GetPivot();
		
		float aspect = !Mathf.Approximately(sprite.height, 0) ? sprite.width / sprite.height : 1;
		
		Rect uv = new Rect(0, 0, 1, 1);
		
		switch (_ScaleMode)
		{
			case ScaleMode.Fit:
				_Rect = MathUtility.Fit(_Rect, aspect, pivot);
				break;
			case ScaleMode.Fill:
				_Rect = MathUtility.Fill(_Rect, aspect, pivot);
				break;
			case ScaleMode.Crop:
				uv = MathUtility.Fit(uv, _Rect.width / _Rect.height, pivot);
				break;
		}
		
		_Vertices.Clear();
		_UV.Clear();
		_Triangles.Clear();
		
		Vector4 vertexBorder = GetVertexBorder(_Border, _BorderMode, _BorderScale, _Rect);
		Vector4 uvBorder     = GetUVBorder(_Border, width, height);
		
		_HorizontalVertex.Clear();
		_VerticalVertex.Clear();
		
		_HorizontalUV.Clear();
		_VerticalUV.Clear();
		
		_HorizontalVertex.Add(_Rect.xMin);
		_VerticalVertex.Add(_Rect.yMax);
		
		_HorizontalUV.Add(uv.xMin);
		_VerticalUV.Add(uv.yMax);
		
		if (_ScaleMode == ScaleMode.Stretch)
		{
			if (_Border.x > float.Epsilon)
			{
				_HorizontalVertex.Add(_Rect.xMin + vertexBorder.x);
				_HorizontalUV.Add(uv.xMin + uvBorder.x);
			}
			
			if (_Border.y > float.Epsilon)
			{
				_HorizontalVertex.Add(_Rect.xMax - vertexBorder.y);
				_HorizontalUV.Add(uv.xMax - uvBorder.y);
			}
			
			if (_Border.z > float.Epsilon)
			{
				_VerticalVertex.Add(_Rect.yMax - vertexBorder.z);
				_VerticalUV.Add(uv.yMax - uvBorder.z);
			}
			
			if (_Border.w > float.Epsilon)
			{
				_VerticalVertex.Add(_Rect.yMin + vertexBorder.w);
				_VerticalUV.Add(uv.yMin + uvBorder.w);
			}
		}
		
		_HorizontalVertex.Add(_Rect.xMax);
		_VerticalVertex.Add(_Rect.yMin);
		
		_HorizontalUV.Add(uv.xMax);
		_VerticalUV.Add(uv.yMin);
		
		int hCenter = (_HorizontalVertex.Count - 1) / 2;
		int vCenter = (_VerticalVertex.Count - 1) / 2;
		
		int position = _Vertices.Count;
		
		for (int y = 0; y < _VerticalVertex.Count - 1; y++)
		for (int x = 0; x < _HorizontalVertex.Count - 1; x++)
		{
			int index = position + y * _HorizontalVertex.Count + x;
			
			if (!_FillCenter && x == hCenter && y == vCenter)
				continue;
			
			_Triangles.Add(index);
			_Triangles.Add(index + 1);
			_Triangles.Add(index + _HorizontalVertex.Count);
			
			_Triangles.Add(index + _HorizontalVertex.Count);
			_Triangles.Add(index + 1);
			_Triangles.Add(index + _HorizontalVertex.Count + 1);
		}
		
		FillUV(
			_Sprite,
			_HorizontalVertex,
			_VerticalVertex,
			_HorizontalUV,
			_VerticalUV,
			_UV
		);
		
		foreach (float y in _VerticalVertex)
		foreach (float x in _HorizontalVertex)
			_Vertices.Add(new Vector3(x, y));
	}

	public static Rect GetUV(Sprite _Sprite)
	{
		Rect uv = new Rect(0, 0, 1, 1);
		if (_Sprite != null && _Sprite.texture != null)
		{
			uv.x      = _Sprite.textureRect.x / _Sprite.texture.width;
			uv.y      = _Sprite.textureRect.y / _Sprite.texture.height;
			uv.width  = _Sprite.textureRect.width / _Sprite.texture.width;
			uv.height = _Sprite.textureRect.height / _Sprite.texture.height;
		}
		return uv;
	}

	public static void FillUV(
		Sprite        _Sprite,
		List<float>   _HorizontalVertex,
		List<float>   _VerticalVertex,
		List<float>   _HorizontalUV,
		List<float>   _VerticalUV,
		List<Vector2> _UV
	)
	{
		FillUV(
			GetUV(_Sprite),
			_HorizontalVertex,
			_VerticalVertex,
			_HorizontalUV,
			_VerticalUV,
			_UV
		);
	}

	public static void FillUV(
		Rect          _Rect,
		List<float>   _HorizontalVertex,
		List<float>   _VerticalVertex,
		List<float>   _HorizontalUV,
		List<float>   _VerticalUV,
		List<Vector2> _UV
	)
	{
		_UV.Clear();
		
		for (int y = 0; y < _VerticalVertex.Count; y++)
		for (int x = 0; x < _HorizontalVertex.Count; x++)
			_UV.Add(new Vector2(_Rect.x + _HorizontalUV[x] * _Rect.width, _Rect.y + _VerticalUV[y] * _Rect.height));
	}

	static Vector4 GetVertexBorder(Vector4 _Border, BorderMode _BorderMode, float _BorderScale, Rect _Rect)
	{
		float width  = Mathf.Abs(_Rect.width);
		float height = Mathf.Abs(_Rect.height);
		
		Vector4 border;
		switch (_BorderMode)
		{
			case BorderMode.Fit:
				float hFit = _Border.x + _Border.y;
				float vFit = _Border.z + _Border.w;
				
				if (hFit + vFit < float.Epsilon)
				{
					border = _Border;
					break;
				}
				
				float fit = Mathf.Min(
					width / (hFit > float.Epsilon ? hFit : vFit),
					height / (vFit > float.Epsilon ? vFit : hFit)
				);
				
				border = _Border * fit;
				break;
			
			case BorderMode.Fill:
				float hFill = _Border.x + _Border.x;
				float vFill = _Border.z + _Border.w;
				
				if (hFill + vFill < float.Epsilon)
				{
					border = _Border;
					break;
				}
				
				float fill = Mathf.Max(
					width / (hFill > float.Epsilon ? hFill : vFill),
					height / (vFill > float.Epsilon ? vFill : hFill)
				);
				border = _Border * fill;
				break;
			
			default:
				border = _Border;
				break;
		}
		
		border *= _BorderScale;
		
		float horizontal = border.x + border.y;
		float vertical   = border.z + border.w;
		
		float xDirection = Mathf.Sign(_Rect.width);
		float yDirection = Mathf.Sign(_Rect.height);
		
		return new Vector4(
			Mathf.Min(width, horizontal) * (border.x / horizontal) * xDirection,
			Mathf.Min(width, horizontal) * (border.y / horizontal) * xDirection,
			Mathf.Min(height, vertical) * (border.z / vertical) * yDirection,
			Mathf.Min(height, vertical) * (border.w / vertical) * yDirection
		);
	}

	static Vector4 GetUVBorder(Vector4 _Border, float _Width, float _Height)
	{
		return new Vector4(
			_Border.x / _Width,
			_Border.y / _Width,
			_Border.z / _Height,
			_Border.w / _Height
		);
	}
}

public static class MathUtility
{
	public static bool Approximately(double _A, double _B)
	{
		return Math.Abs(_A - _B) < double.Epsilon * 2;
	}

	public static float Remap(float _Value, float _Low1, float _High1, float _Low2, float _High2)
	{
		return _Low2 + (_Value - _Low1) * (_High2 - _Low2) / (_High1 - _Low1);
	}

	public static long Remap(long _Value, long _Low1, long _High1, long _Low2, long _High2)
	{
		return (long)(_Low2 + (_Value - _Low1) * (_High2 - _Low2) / (double)(_High1 - _Low1));
	}

	public static double Remap(double _Value, double _Low1, double _High1, double _Low2, double _High2)
	{
		return _Low2 + (_Value - _Low1) * (_High2 - _Low2) / (_High1 - _Low1);
	}

	public static float RemapClamped(float _Value, float _Low1, float _High1, float _Low2, float _High2)
	{
		return Mathf.Clamp(Remap(_Value, _Low1, _High1, _Low2, _High2), _Low2, _High2);
	}

	public static double RemapClamped(double _Value, double _Low1, double _High1, double _Low2, double _High2)
	{
		return Math.Clamp(Remap(_Value, _Low1, _High1, _Low2, _High2), _Low2, _High2);
	}

	public static long RemapClamped(long _Value, long _Low1, long _High1, long _Low2, long _High2)
	{
		return Math.Clamp(Remap(_Value, _Low1, _High1, _Low2, _High2), _Low2, _High2);
	}

	public static float Remap01(float _Value, float _Low, float _High)
	{
		return (_Value - _Low) / (_High - _Low);
	}

	public static float Remap01(long _Value, long _Low, long _High)
	{
		return Remap01((double)_Value, _Low, _High);
	}

	public static float Remap01(int _Value, int _Low, int _High)
	{
		return Remap01((double)_Value, _Low, _High);
	}

	public static float Remap01(double _Value, double _Low, double _High)
	{
		return (float)((_Value - _Low) / (_High - _Low));
	}

	public static float Remap01Clamped(float _Value, float _Low, float _High)
	{
		return Mathf.Clamp01(Remap01(_Value, _Low, _High));
	}

	public static double Remap01Clamped(double _Value, double _Low, double _High)
	{
		return Mathf.Clamp01(Remap01(_Value, _Low, _High));
	}

	public static float Snap(float _Value, float _Step)
	{
		return Mathf.Round(_Value / _Step) * _Step;
	}

	public static float Snap(float _Value, float _Min, float _Max, params float[] _Steps)
	{
		if (_Steps.Length == 0)
			return _Value;
		
		const float threshold = 200;
		
		float length = _Max - _Min;
		
		int index = -1;
		
		for (int i = _Steps.Length - 1; i >= 0; i--)
		{
			int count = (int)(length / _Steps[i]);
			
			if (count > threshold)
				break;
			
			index = i;
		}
		
		return index >= 0 && index < _Steps.Length ? Snap(_Value, _Steps[index]) : _Value;
	}

	public static Rect Round(Rect _Rect)
	{
		return new Rect(
			Mathf.Round(_Rect.x),
			Mathf.Round(_Rect.y),
			Mathf.Round(_Rect.width),
			Mathf.Round(_Rect.height)
		);
	}

	public static Rect Fit(Rect _Rect, float _Aspect)
	{
		return Fit(_Rect, _Aspect, new Vector2(0.5f, 0.5f));
	}

	public static Vector2 Fit(Vector2 _Vector, float _Aspect)
	{
		Vector2 h = new Vector2(_Vector.x, _Vector.x / _Aspect);
		Vector2 v = new Vector2(_Vector.y * _Aspect, _Vector.y);
		
		return h.x * h.y <= v.x * v.y ? h : v;
	}

	public static Rect Fit(Rect _Rect, float _Aspect, Vector2 _Pivot)
	{
		Vector2 h = new Vector2(_Rect.width, _Rect.width / _Aspect);
		Vector2 v = new Vector2(_Rect.height * _Aspect, _Rect.height);
		
		Vector2 size     = Mathf.Abs(h.x * h.y) <= Mathf.Abs(v.x * v.y) ? h : v;
		Vector2 position = _Rect.position + Vector2.Scale(_Rect.size - size, _Pivot);
		
		return new Rect(position, size);
	}

	public static Rect Fill(Rect _Rect, float _Aspect)
	{
		return Fill(_Rect, _Aspect, new Vector2(0.5f, 0.5f));
	}

	public static Rect Fill(Rect _Rect, float _Aspect, Vector2 _Pivot)
	{
		Vector2 h = new Vector2(_Rect.width, _Rect.width / _Aspect);
		Vector2 v = new Vector2(_Rect.height * _Aspect, _Rect.height);
		
		Vector2 size     = Mathf.Abs(h.x * h.y) >= Mathf.Abs(v.x * v.y) ? h : v;
		Vector2 position = _Rect.position + Vector2.Scale(_Rect.size - size, _Pivot);
		
		return new Rect(position, size);
	}

	public static Vector2 Fill(Vector2 _Vector, float _Aspect)
	{
		Vector2 h = new Vector2(_Vector.x, _Vector.x / _Aspect);
		Vector2 v = new Vector2(_Vector.y * _Aspect, _Vector.y);
		
		return Mathf.Abs(h.x * h.y) >= Mathf.Abs(v.x * v.y) ? h : v;
	}

	public static Rect Uniform(Rect _Source, Rect _Target)
	{
		Rect rect = new Rect(
			Remap01(_Source.x, _Target.xMin, _Target.xMax),
			Remap01(_Source.y, _Target.yMin, _Target.yMax),
			_Source.width / _Target.width,
			_Source.height / _Target.height
		);
		
		return Fit(rect, _Target.width / _Target.height);
	}

	public static int Repeat(int _Value, int _Length)
	{
		int value = _Value % _Length;
		return value < 0 ? value + _Length : value;
	}

	public static int Repeat(int _Value, int _Min, int _Max)
	{
		int value  = _Value - _Min;
		int length = _Max - _Min;
		return _Min + Repeat(value, length + 1);
	}

	public static int Lerp(int _Source, int _Target, float _Phase)
	{
		return _Source + (int)((_Target - _Source) * _Phase);
	}

	public static Rect Lerp(Rect _Source, Rect _Target, float _Phase)
	{
		return new Rect(
			Vector2.Lerp(_Source.position, _Target.position, _Phase),
			Vector2.Lerp(_Source.size, _Target.size, _Phase)
		);
	}

	public static long Lerp(long _Source, long _Target, float _Phase)
	{
		return _Source + (long)((_Target - _Source) * (double)_Phase);
	}

	public static double Lerp(double _Source, double _Target, float _Phase)
	{
		return _Source + (_Target - _Source) * _Phase;
	}
}
