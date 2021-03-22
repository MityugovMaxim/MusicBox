using UnityEngine;
using UnityEngine.UI;

public class CircleImage : MaskableGraphic
{
	public enum Direction
	{
		All   = 0,
		Left  = 1,
		Right = 2,
		Up    = 3,
		Down  = 4,
	}

	public float Radius
	{
		get => m_Radius;
		set
		{
			if (Mathf.Approximately(m_Radius, value))
				return;
			
			m_Radius = value;
			
			SetVerticesDirty();
		}
	}

	public override Texture mainTexture => m_Pattern != null ? m_Pattern : s_WhiteTexture;

	[SerializeField]              Direction m_Direction;
	[SerializeField]              Texture   m_Pattern;
	[SerializeField, Range(0, 1)] float     m_Radius    = 0.5f;
	[SerializeField, Range(0, 1)] float     m_Thickness = 0.1f;
	[SerializeField, Range(0, 1)] float     m_Smooth    = 0.05f;
	[SerializeField]              int       m_Size      = 40;

	readonly UIVertex[] m_Vertices =
	{
		new UIVertex(),
		new UIVertex(),
		new UIVertex(),
		new UIVertex(),
	};

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		Rect    rect = GetPixelAdjustedRect();
		Vector4 quad = new Vector4(rect.x, rect.y, rect.x + rect.width, rect.y + rect.height);
		
		Vector2 data0  = new Vector2(m_Radius, m_Thickness);
		Vector2 data1 = new Vector2(m_Smooth, m_Size);
		
		float aspect = rect.width / rect.height * 0.5f;
		
		Color c = color;
		
		m_Vertices[0].position = new Vector3(quad.x, quad.y);
		m_Vertices[0].color    = c;
		m_Vertices[0].uv0      = new Vector2(0.5f - aspect, 0);
		m_Vertices[0].uv1      = data0;
		m_Vertices[0].uv2      = data1;
		
		m_Vertices[1].position = new Vector3(quad.x, quad.w);
		m_Vertices[1].color    = c;
		m_Vertices[1].uv0      = new Vector2(0.5f - aspect, 1);
		m_Vertices[1].uv1      = data0;
		m_Vertices[1].uv2      = data1;
		
		m_Vertices[2].position = new Vector3(quad.z, quad.w);
		m_Vertices[2].color    = c;
		m_Vertices[2].uv0      = new Vector2(0.5f + aspect, 1);
		m_Vertices[2].uv1      = data0;
		m_Vertices[2].uv2      = data1;
		
		m_Vertices[3].position = new Vector3(quad.z, quad.y);
		m_Vertices[3].color    = c;
		m_Vertices[3].uv0      = new Vector2(0.5f + aspect, 0);
		m_Vertices[3].uv1      = data0;
		m_Vertices[3].uv2      = data1;
		
		switch (m_Direction)
		{
			case Direction.Left:
				LeftDirection(quad, m_Vertices);
				break;
			case Direction.Right:
				RightDirection(quad, m_Vertices);
				break;
			case Direction.Up:
				UpDirection(quad, m_Vertices);
				break;
			case Direction.Down:
				DownDirection(quad, m_Vertices);
				break;
		}
		
		_VertexHelper.Clear();
		_VertexHelper.AddVert(m_Vertices[0]);
		_VertexHelper.AddVert(m_Vertices[1]);
		_VertexHelper.AddVert(m_Vertices[2]);
		_VertexHelper.AddVert(m_Vertices[3]);
		
		_VertexHelper.AddTriangle(0, 1, 2);
		_VertexHelper.AddTriangle(2, 3, 0);
	}

	static void LeftDirection(Vector4 _Quad, UIVertex[] _Vertices)
	{
		Vector3 position = new Vector3(_Quad.x + _Quad.z, _Quad.y + _Quad.w) * 0.5f;
		Vector2 uv = new Vector2(0.5f, 0.5f);
		
		_Vertices[2].position = position;
		_Vertices[2].uv0      = uv;
		
		_Vertices[3].position = position;
		_Vertices[3].uv0      = uv;
	}

	static void RightDirection(Vector4 _Quad, UIVertex[] _Vertices)
	{
		Vector3 position = new Vector3(_Quad.x + _Quad.z, _Quad.y + _Quad.w) * 0.5f;
		Vector2 uv       = new Vector2(0.5f, 0.5f);
		
		_Vertices[0].position = position;
		_Vertices[0].uv0      = uv;
		
		_Vertices[1].position = position;
		_Vertices[1].uv0      = uv;
	}

	static void UpDirection(Vector4 _Quad, UIVertex[] _Vertices)
	{
		Vector3 position = new Vector3(_Quad.x + _Quad.z, _Quad.y + _Quad.w) * 0.5f;
		Vector2 uv       = new Vector2(0.5f, 0.5f);
		
		_Vertices[0].position = position;
		_Vertices[0].uv0      = uv;
		
		_Vertices[3].position = position;
		_Vertices[3].uv0      = uv;
	}

	static void DownDirection(Vector4 _Quad, UIVertex[] _Vertices)
	{
		Vector3 position = new Vector3(_Quad.x + _Quad.z, _Quad.y + _Quad.w) * 0.5f;
		Vector2 uv       = new Vector2(0.5f, 0.5f);
		
		_Vertices[1].position = position;
		_Vertices[1].uv0      = uv;
		
		_Vertices[2].position = position;
		_Vertices[2].uv0      = uv;
	}
}