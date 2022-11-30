using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuadGenerator
{
	readonly List<Vector3> m_Vertices         = new List<Vector3>();
	readonly List<Vector2> m_UV               = new List<Vector2>();
	readonly List<int>     m_Triangles        = new List<int>();
	readonly List<float>   m_HorizontalVertex = new List<float>();
	readonly List<float>   m_VerticalVertex   = new List<float>();
	readonly List<float>   m_HorizontalUV     = new List<float>();
	readonly List<float>   m_VerticalUV       = new List<float>();

	public void Generate(
		Rect       _Rect,
		Vector2    _Size,
		TextAnchor _Alignment,
		ScaleMode  _ScaleMode,
		BorderMode _BorderMode,
		Vector4    _Border,
		float      _BorderScale,
		bool       _FillCenter
	)
	{
		Vector2 pivot = _Alignment.GetPivot();
		
		float aspect = !Mathf.Approximately(_Size.y, 0) ? _Size.x / _Size.y : 1;
		
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
		
		m_Vertices.Clear();
		m_UV.Clear();
		m_Triangles.Clear();
		
		Vector4 vertexBorder = GetVertexBorder(_BorderMode, _Border, _BorderScale, _Rect);
		Vector4 uvBorder     = GetUVBorder(_Border, _Size.x, _Size.y);
		
		m_HorizontalVertex.Clear();
		m_VerticalVertex.Clear();
		
		m_HorizontalUV.Clear();
		m_VerticalUV.Clear();
		
		m_HorizontalVertex.Add(_Rect.xMin);
		m_VerticalVertex.Add(_Rect.yMax);
		
		m_HorizontalUV.Add(uv.xMin);
		m_VerticalUV.Add(uv.yMax);
		
		if (_ScaleMode == ScaleMode.Stretch)
		{
			if (vertexBorder.x > float.Epsilon)
			{
				m_HorizontalVertex.Add(_Rect.xMin + vertexBorder.x);
				m_HorizontalUV.Add(uv.xMin + uvBorder.x);
			}
			
			if (vertexBorder.y > float.Epsilon)
			{
				m_HorizontalVertex.Add(_Rect.xMax - vertexBorder.y);
				m_HorizontalUV.Add(uv.xMax - uvBorder.y);
			}
			
			if (vertexBorder.z > float.Epsilon)
			{
				m_VerticalVertex.Add(_Rect.yMax - vertexBorder.z);
				m_VerticalUV.Add(uv.yMax - uvBorder.z);
			}
			
			if (vertexBorder.w > float.Epsilon)
			{
				m_VerticalVertex.Add(_Rect.yMin + vertexBorder.w);
				m_VerticalUV.Add(uv.yMin + uvBorder.w);
			}
		}
		
		m_HorizontalVertex.Add(_Rect.xMax);
		m_VerticalVertex.Add(_Rect.yMin);
		
		m_HorizontalUV.Add(uv.xMax);
		m_VerticalUV.Add(uv.yMin);
		
		int hCenter = (m_HorizontalVertex.Count - 1) / 2;
		int vCenter = (m_VerticalVertex.Count - 1) / 2;
		
		int position = m_Vertices.Count;
		
		for (int y = 0; y < m_VerticalVertex.Count - 1; y++)
		for (int x = 0; x < m_HorizontalVertex.Count - 1; x++)
		{
			int index = position + y * m_HorizontalVertex.Count + x;
			
			if (!_FillCenter && x == hCenter && y == vCenter)
				continue;
			
			m_Triangles.Add(index);
			m_Triangles.Add(index + 1);
			m_Triangles.Add(index + m_HorizontalVertex.Count);
			
			m_Triangles.Add(index + m_HorizontalVertex.Count);
			m_Triangles.Add(index + 1);
			m_Triangles.Add(index + m_HorizontalVertex.Count + 1);
		}
		
		foreach (float y in m_VerticalVertex)
		foreach (float x in m_HorizontalVertex)
			m_Vertices.Add(new Vector3(x, y));
	}

	public void RemapUV(Sprite _Sprite, List<Vector2> _UV)
	{
		RemapUV(MeshUtility.GetUV(_Sprite), _UV);
	}

	public void RemapUV(Rect _Rect, List<Vector2> _UV)
	{
		_UV.Clear();
		
		for (int y = 0; y < m_VerticalVertex.Count; y++)
		for (int x = 0; x < m_HorizontalVertex.Count; x++)
			_UV.Add(new Vector2(_Rect.x + m_HorizontalUV[x] * _Rect.width, _Rect.y + m_VerticalUV[y] * _Rect.height));
	}

	public void Fill(VertexHelper _VertexHelper, Sprite _Sprite, Color32 _Color)
	{
		_VertexHelper.Clear();
		
		RemapUV(_Sprite, m_UV);
		
		for (int i = 0; i < m_Vertices.Count; i++)
		{
			_VertexHelper.AddVert(
				m_Vertices[i],
				_Color,
				m_UV[i]
			);
		}
		
		for (int i = 2; i < m_Triangles.Count; i += 3)
			_VertexHelper.AddTriangle(m_Triangles[i - 2], m_Triangles[i - 1], m_Triangles[i - 0]);
	}

	static Vector4 GetVertexBorder(BorderMode _BorderMode, Vector4 _Border, float _BorderScale, Rect _Rect)
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
