using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UIRounded : SpriteGraphic
{
	public enum Mode
	{
		Inner,
		Outer,
		Center,
	}

	public float Radius
	{
		get => m_Radius.x;
		set
		{
			Vector4 radius = new Vector4(value, value, value, value);
			if (m_Radius == radius)
				return;
			
			m_Radius     = radius;
			m_ValidShape = false;
			
			SetVerticesDirty();
		}
	}

	public float TopLeftRadius
	{
		get => m_Radius.x;
		set
		{
			if (Mathf.Approximately(m_Radius.x, value))
				return;
			
			m_Radius.x   = value;
			m_ValidShape = false;
			
			SetVerticesDirty();
		}
	}

	public float TopRightRadius
	{
		get => m_Radius.y;
		set
		{
			if (Mathf.Approximately(m_Radius.y, value))
				return;
			
			m_Radius.y   = value;
			m_ValidShape = false;
			
			SetVerticesDirty();
		}
	}

	public float BottomLeftRadius
	{
		get => m_Radius.z;
		set
		{
			if (Mathf.Approximately(m_Radius.z, value))
				return;
			
			m_Radius.z   = value;
			m_ValidShape = false;
			
			SetVerticesDirty();
		}
	}

	public float BottomRightRadius
	{
		get => m_Radius.w;
		set
		{
			if (Mathf.Approximately(m_Radius.w, value))
				return;
			
			m_Radius.w   = value;
			m_ValidShape = false;
			
			SetVerticesDirty();
		}
	}

	public float Width
	{
		get => m_Width;
		set
		{
			if (Mathf.Approximately(m_Width, value))
				return;
			
			m_Width      = value;
			m_ValidShape = false;
			
			SetVerticesDirty();
		}
	}

	[SerializeField] Vector4 m_Radius;

	[SerializeField] float    m_Width;
	[SerializeField] Gradient m_Gradient;
	[SerializeField] Mode     m_Mode;

	readonly List<UIVertex> m_Vertices = new List<UIVertex>();
	readonly List<int>      m_Indices  = new List<int>();

	bool m_ValidShape;

	protected override void OnEnable()
	{
		m_ValidShape = false;
		
		base.OnEnable();
	}

	float GetOffset()
	{
		float weight;
		switch (m_Mode)
		{
			case Mode.Inner:
				weight = 1;
				break;
			case Mode.Center:
				weight = 0.5f;
				break;
			case Mode.Outer:
				weight = 0;
				break;
			default:
				weight = 0;
				break;
		}
		return weight * m_Width;
	}

	Rect m_CachedRect;

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		_VertexHelper.Clear();
		
		Color32 colorCache = color;
		
		if (m_ValidShape)
		{
			
			for (int i = 0; i < m_Vertices.Count; i++)
			{
				UIVertex vertex = m_Vertices[i];
				vertex.color  = colorCache;
				m_Vertices[i] = vertex;
			}
			_VertexHelper.AddUIVertexStream(m_Vertices, m_Indices);
			return;
		}
		
		m_ValidShape = true;
		
		m_Vertices.Clear();
		m_Indices.Clear();
		
		Rect rect = rectTransform.rect;
		
		float offset = GetOffset();
		
		rect.xMin += offset;
		rect.xMax -= offset;
		rect.yMin += offset;
		rect.yMax -= offset;
		
		float tlRadius = ConstrainTopLeftRadius(rect);
		float trRadius = ConstrainTopRightRadius(rect);
		float brRadius = ConstrainBottomRightRadius(rect);
		float blRadius = ConstrainBottomLeftRadius(rect);
		
		float totalLength = 0;
		
		totalLength += Mathf.PI * tlRadius * 0.5f;
		totalLength += Mathf.PI * trRadius * 0.5f;
		totalLength += Mathf.PI * brRadius * 0.5f;
		totalLength += Mathf.PI * blRadius * 0.5f;
		
		totalLength += rect.height - tlRadius - blRadius;
		totalLength += rect.height - trRadius - brRadius;
		totalLength += rect.width - tlRadius - trRadius;
		totalLength += rect.width - blRadius - brRadius;
		
		float length = 0;
		
		length += TopLeftCorner(tlRadius, totalLength, length, rect, _VertexHelper);
		
		length += TopSide(tlRadius, trRadius, totalLength, length, rect, _VertexHelper);
		
		length += TopRightCorner(trRadius, totalLength, length, rect, _VertexHelper);
		
		length += RightSide(trRadius, brRadius, totalLength, length, rect, _VertexHelper);
		
		length += BottomRightCorner(brRadius, totalLength, length, rect, _VertexHelper);
		
		length += BottomSide(brRadius, blRadius, totalLength, length, rect, _VertexHelper);
		
		length += BottomLeftCorner(blRadius, totalLength, length, rect, _VertexHelper);
		
		length += LeftSide(blRadius, tlRadius, totalLength, length, rect, _VertexHelper);
		
		int quads = m_Vertices.Count / 2 - 1;
		for (int i = 0; i < quads; i++)
		{
			m_Indices.Add(i * 2 + 2);
			m_Indices.Add(i * 2 + 3);
			m_Indices.Add(i * 2 + 0);
			
			m_Indices.Add(i * 2 + 0);
			m_Indices.Add(i * 2 + 3);
			m_Indices.Add(i * 2 + 1);
		}
		
		_VertexHelper.AddUIVertexStream(m_Vertices, m_Indices);
	}

	float ConstrainTopLeftRadius(Rect _Rect) => ConstrainRadius(TopLeftRadius, TopRightRadius, BottomLeftRadius, _Rect);

	float ConstrainTopRightRadius(Rect _Rect) => ConstrainRadius(TopRightRadius, TopLeftRadius, BottomRightRadius, _Rect);

	float ConstrainBottomLeftRadius(Rect _Rect) => ConstrainRadius(BottomLeftRadius, BottomRightRadius, TopLeftRadius, _Rect);

	float ConstrainBottomRightRadius(Rect _Rect) => ConstrainRadius(BottomRightRadius, BottomLeftRadius, TopRightRadius, _Rect);

	static float ConstrainRadius(float _Radius, float _HRadius, float _VRadius, Rect _Rect)
	{
		float dimension;
		float size;
		if (_Rect.width < _Rect.height)
		{
			size      = _Radius + _HRadius;
			dimension = _Rect.width;
		}
		else
		{
			size      = _Radius + _VRadius;
			dimension = _Rect.height;
		}
		return size > dimension
			? _Radius / size * dimension
			: _Radius;
	}

	float TopLeftCorner(float _Radius, float _TotalLength, float _Length, Rect _Rect, VertexHelper _VertexHelper)
	{
		return Corner(
			new Vector2(-1, 0),
			new Vector2(_Rect.xMin + _Radius, _Rect.yMax - _Radius),
			_Radius,
			_TotalLength,
			_Length
		);
	}

	float TopRightCorner(float _Radius, float _TotalLength, float _Length, Rect _Rect, VertexHelper _VertexHelper)
	{
		return Corner(
			new Vector2(0, 1),
			new Vector2(_Rect.xMax - _Radius, _Rect.yMax - _Radius),
			_Radius,
			_TotalLength,
			_Length
		);
	}

	float BottomRightCorner(float _Radius, float _TotalLength, float _Length, Rect _Rect, VertexHelper _VertexHelper)
	{
		return Corner(
			new Vector2(1, 0),
			new Vector2(_Rect.xMax - _Radius, _Rect.yMin + _Radius),
			_Radius,
			_TotalLength,
			_Length
		);
	}

	float BottomLeftCorner(float _Radius, float _TotalLength, float _Length, Rect _Rect, VertexHelper _VertexHelper)
	{
		return Corner(
			new Vector2(0, -1),
			new Vector2(_Rect.xMin + _Radius, _Rect.yMin + _Radius),
			_Radius,
			_TotalLength,
			_Length
		);
	}

	float TopSide(float _TLRadius, float _TRRadius, float _TotalLength, float _Length, Rect _Rect, VertexHelper _VertexHelper)
	{
		return Side(
			new Vector2(0, 1),
			new Vector2(_Rect.xMin + _TLRadius, _Rect.yMax),
			new Vector2(_Rect.xMax - _TRRadius, _Rect.yMax),
			_TotalLength,
			_Length
		);
	}

	float RightSide(float _TRRadius, float _BRRadius, float _TotalLength, float _Length, Rect _Rect, VertexHelper _VertexHelper)
	{
		return Side(
			new Vector2(1, 0),
			new Vector2(_Rect.xMax, _Rect.yMax - _TRRadius),
			new Vector2(_Rect.xMax, _Rect.yMin + _BRRadius),
			_TotalLength,
			_Length
		);
	}

	float BottomSide(float _BRRadius, float _BLRadius, float _TotalLength, float _Length, Rect _Rect, VertexHelper _VertexHelper)
	{
		return Side(
			new Vector2(0, -1),
			new Vector2(_Rect.xMax - _BRRadius, _Rect.yMin),
			new Vector2(_Rect.xMin + _BLRadius, _Rect.yMin),
			_TotalLength,
			_Length
		);
	}

	float LeftSide(float _BLRadius, float _TLRadius, float _TotalLength, float _Length, Rect _Rect, VertexHelper _VertexHelper)
	{
		return Side(
			new Vector2(-1, 0),
			new Vector2(_Rect.xMin, _Rect.yMin + _BLRadius),
			new Vector2(_Rect.xMin, _Rect.yMax - _TLRadius),
			_TotalLength,
			_Length
		);
	}

	float Corner(Vector2 _Normal, Vector2 _Pivot, float   _Radius, float   _TotalLength, float   _Length)
	{
		float length = Mathf.PI * _Radius * 0.5f;
		
		int count = Mathf.FloorToInt(length / 4);
		int quads = count - 1;
		
		Quaternion rotation = Quaternion.Euler(0, 0, -90.0f / quads);
		
		Vector2 inner = _Normal * _Radius;
		Vector2 outer = _Normal * (_Radius + m_Width);
		Vector2 size  = new Vector2(_TotalLength, m_Width);
		
		for (int i = 0; i < count; i++)
		{
			float phase  = (float)i / quads;
			float lPhase = (_Length + length * phase) / _TotalLength;
			
			Vector2 innerPoint = _Pivot + inner;
			Vector2 outerPoint = _Pivot + outer;
			
			Vector2 innerUV = new Vector2(lPhase, 0);
			Vector2 outerUV = new Vector2(lPhase, 1);
			
			Color color = m_Gradient.Evaluate(lPhase) * base.color;
			
			m_Vertices.Add(new UIVertex() { position = innerPoint, color = color, uv0 = GetSpriteUV(innerUV), uv1 = innerUV, uv2 = size });
			m_Vertices.Add(new UIVertex() { position = outerPoint, color = color, uv0 = GetSpriteUV(outerUV), uv1 = outerUV, uv2 = size });
			
			inner = rotation * inner;
			outer = rotation * outer;
		}
		
		return length;
	}

	float Side(Vector2 _Normal, Vector2 _Source, Vector2 _Target, float _TotalLength, float _Length)
	{
		const float threshold = 10;
		
		Vector2 delta  = _Target - _Source;
		float   length = Mathf.Abs(delta.x * _Normal.y) + Mathf.Abs(delta.y * _Normal.x);
		Vector2 size   = new Vector2(_TotalLength, m_Width);
		
		int count = Mathf.Max(2, Mathf.CeilToInt(length / threshold));
		int quads = count - 1;
		for (int i = 0; i < count; i++)
		{
			float   phase = (float)i / quads;
			Vector2 point = _Source + delta * phase;
			
			float lPhase = (_Length + length * phase) / _TotalLength;
			
			Vector2 innerPoint = point;
			Vector2 outerPoint = point + _Normal * m_Width;
			
			Vector2 innerUV = new Vector2(lPhase, 0);
			Vector2 outerUV = new Vector2(lPhase, 1);
			
			Color color = m_Gradient.Evaluate(lPhase) * base.color;
			
			m_Vertices.Add(new UIVertex() { position = innerPoint, color = color, uv0 = GetSpriteUV(innerUV), uv1 = innerUV, uv2 = size });
			m_Vertices.Add(new UIVertex() { position = outerPoint, color = color, uv0 = GetSpriteUV(outerUV), uv1 = outerUV, uv2 = size });
		}
		
		return length;
	}
}