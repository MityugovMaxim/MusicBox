using UnityEngine;
using UnityEngine.UI;

public class Indicator : MaskableGraphic
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
		Rect source  = GetPixelAdjustedRect();
		Rect target  = MathUtility.Fit(source, 1);
		Rect texture = MathUtility.Uniform(source, target);
		
		float weight = 0.5f - Radius;
		
		Vector4 position = new Vector4(
			target.x + target.width * weight,
			target.y + target.height * weight,
			target.x + target.width - target.width * weight,
			target.y + target.height - target.height * weight
		);
		
		Vector4 uv = new Vector4(
			texture.x + texture.width * weight,
			texture.y + texture.height * weight,
			texture.x + texture.width - texture.width * weight,
			texture.y + texture.height - texture.height * weight
		);
		
		Color vertexColor = color;
		
		Vector2 data0 = new Vector2(m_Radius, m_Thickness);
		Vector2 data1 = new Vector2(m_Smooth, m_Size);
		
		SetupVertex(ref m_Vertices[0], new Vector3(position.x, position.y), vertexColor, new Vector2(uv.x, uv.y), data0, data1);
		SetupVertex(ref m_Vertices[1], new Vector3(position.x, position.w), vertexColor, new Vector2(uv.x, uv.w), data0, data1);
		SetupVertex(ref m_Vertices[2], new Vector3(position.z, position.w), vertexColor, new Vector2(uv.z, uv.w), data0, data1);
		SetupVertex(ref m_Vertices[3], new Vector3(position.z, position.y), vertexColor, new Vector2(uv.z, uv.y), data0, data1);
		
		switch (m_Direction)
		{
			case Direction.Left:
				LeftDirection(position, m_Vertices);
				break;
			case Direction.Right:
				RightDirection(position, m_Vertices);
				break;
			case Direction.Up:
				UpDirection(position, m_Vertices);
				break;
			case Direction.Down:
				DownDirection(position, m_Vertices);
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

	static void SetupVertex(ref UIVertex _Vertex, Vector3 _Position, Color _Color, Vector2 _UV0, Vector2 _UV1, Vector2 _UV2)
	{
		_Vertex.position = _Position;
		_Vertex.color    = _Color;
		_Vertex.uv0      = _UV0;
		_Vertex.uv1      = _UV1;
		_Vertex.uv2      = _UV2;
		_Vertex.normal   = Vector3.back;
	}

	static void LeftDirection(Vector4 _Position, UIVertex[] _Vertices)
	{
		Vector3 position = new Vector3(_Position.x + _Position.z, _Position.y + _Position.w) * 0.5f;
		Vector2 uv = new Vector2(0.5f, 0.5f);
		
		_Vertices[2].position = position;
		_Vertices[2].uv0      = uv;
		
		_Vertices[3].position = position;
		_Vertices[3].uv0      = uv;
	}

	static void RightDirection(Vector4 _Position, UIVertex[] _Vertices)
	{
		Vector3 position = new Vector3(_Position.x + _Position.z, _Position.y + _Position.w) * 0.5f;
		Vector2 uv       = new Vector2(0.5f, 0.5f);
		
		_Vertices[0].position = position;
		_Vertices[0].uv0      = uv;
		
		_Vertices[1].position = position;
		_Vertices[1].uv0      = uv;
	}

	static void UpDirection(Vector4 _Position, UIVertex[] _Vertices)
	{
		Vector3 position = new Vector3(_Position.x + _Position.z, _Position.y + _Position.w) * 0.5f;
		Vector2 uv       = new Vector2(0.5f, 0.5f);
		
		_Vertices[0].position = position;
		_Vertices[0].uv0      = uv;
		
		_Vertices[3].position = position;
		_Vertices[3].uv0      = uv;
	}

	static void DownDirection(Vector4 _Position, UIVertex[] _Vertices)
	{
		Vector3 position = new Vector3(_Position.x + _Position.z, _Position.y + _Position.w) * 0.5f;
		Vector2 uv       = new Vector2(0.5f, 0.5f);
		
		_Vertices[1].position = position;
		_Vertices[1].uv0      = uv;
		
		_Vertices[2].position = position;
		_Vertices[2].uv0      = uv;
	}
}