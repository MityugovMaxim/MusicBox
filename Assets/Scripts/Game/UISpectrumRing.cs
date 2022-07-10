using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class UISpectrumRing : UIOrder
{
	public enum PositionMode
	{
		Outer,
		Inner,
		Center
	}

	public override int Thickness => 1;

	static readonly int m_MainTexPropertyID = Shader.PropertyToID("_MainTex");
	static readonly int m_ColorPropertyID   = Shader.PropertyToID("_Color");

	[SerializeField] Color        m_Color = Color.white;
	[SerializeField] Sprite       m_Sprite;
	[SerializeField] PositionMode m_PositionMode;
	[SerializeField] float        m_Radius;
	[SerializeField] int          m_Count = 64;
	[SerializeField] float        m_Width;
	[SerializeField] float        m_Height;
	[SerializeField] float        m_Rotation;

	readonly List<Vector3> m_Vertices  = new List<Vector3>();
	readonly List<Vector2> m_UV0       = new List<Vector2>();
	readonly List<Vector4> m_UV1       = new List<Vector4>();
	readonly List<int>     m_Triangles = new List<int>();

	MeshRenderer          m_MeshRenderer;
	MeshFilter            m_MeshFilter;
	MaterialPropertyBlock m_PropertyBlock;
	Mesh                  m_Mesh;
	Vector3               m_Angle;
	Transform             m_Target;

	protected override void Awake()
	{
		base.Awake();
		
		m_MeshFilter    = GetComponent<MeshFilter>();
		m_MeshRenderer  = GetComponent<MeshRenderer>();
		m_PropertyBlock = new MaterialPropertyBlock();
		m_Mesh          = new Mesh();
		m_Target        = RectTransform;
		
		GenerateMesh();
		
		GenerateProperties();
	}

	void Update()
	{
		m_Angle.z += m_Rotation * Time.deltaTime;
		m_Angle.z %= 360;
		m_Target.localEulerAngles = m_Angle;
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (!IsInstanced || Application.isPlaying)
			return;
		
		if (m_MeshRenderer == null)
			m_MeshRenderer = GetComponent<MeshRenderer>();
		
		if (m_MeshFilter == null)
			m_MeshFilter = GetComponent<MeshFilter>();
		
		if (m_PropertyBlock == null)
			m_PropertyBlock = new MaterialPropertyBlock();
		
		if (m_Mesh == null)
			m_Mesh = new Mesh();
		
		GenerateMesh();
		
		GenerateProperties();
	}
	#endif

	void GenerateMesh()
	{
		m_Vertices.Clear();
		m_UV0.Clear();
		m_UV1.Clear();
		m_Triangles.Clear();
		
		Quaternion step = Quaternion.Euler(0, 0, 360.0f / m_Count);
		
		Vector2 normal = Vector2.up;
		
		for (int i = 0; i < m_Count; i++)
		{
			GenerateElement(i, normal);
			
			normal = step * normal;
		}
		
		m_Mesh.Clear();
		m_Mesh.SetVertices(m_Vertices);
		m_Mesh.SetUVs(0, m_UV0);
		m_Mesh.SetUVs(1, m_UV1);
		m_Mesh.SetTriangles(m_Triangles, 0);
		
		m_Mesh.RecalculateBounds();
		m_Mesh.RecalculateNormals();
		
		m_MeshFilter.sharedMesh = m_Mesh;
	}

	void GenerateElement(int _Index, Vector2 _Normal)
	{
		float   size   = m_Width * 0.5f;
		Rect    uv     = MeshUtility.GetUV(m_Sprite);
		Vector2 min    = _Normal * (m_Radius - size);
		Vector2 max    = _Normal * (m_Radius + size);
		Vector2 width  = _Normal.Rotate90() * size;
		Vector2 border = _Normal * size;
		
		// 0 -- -- -- 1
		// |          |
		// 2 -- -- -- 3
		// |          |
		// |          |
		// |          |
		// |          |
		// 4 -- -- -- 5
		// |          |
		// 6 -- -- -- 7
		
		int index = m_Vertices.Count;
		
		m_Vertices.Add(max - width);
		m_Vertices.Add(max + width);
		
		m_Vertices.Add(max - border - width);
		m_Vertices.Add(max - border + width);
		
		m_Vertices.Add(min + border - width);
		m_Vertices.Add(min + border + width);
		
		m_Vertices.Add(min - width);
		m_Vertices.Add(min + width);
		
		m_UV0.Add(new Vector2(uv.xMin, uv.yMax));
		m_UV0.Add(new Vector2(uv.xMax, uv.yMax));
		
		m_UV0.Add(new Vector2(uv.xMin, uv.center.y));
		m_UV0.Add(new Vector2(uv.xMax, uv.center.y));
		
		m_UV0.Add(new Vector2(uv.xMin, uv.center.y));
		m_UV0.Add(new Vector2(uv.xMax, uv.center.y));
		
		m_UV0.Add(new Vector2(uv.xMin, uv.yMin));
		m_UV0.Add(new Vector2(uv.xMax, uv.yMin));
		
		Vector4 upper;
		Vector4 lower;
		switch (m_PositionMode)
		{
			case PositionMode.Outer:
				upper = new Vector4(_Normal.x, _Normal.y, _Index, m_Height);
				lower = new Vector4(_Normal.x, _Normal.y, _Index, 0);
				break;
			case PositionMode.Inner:
				upper = new Vector4(_Normal.x, _Normal.y, _Index, 0);
				lower = new Vector4(_Normal.x, _Normal.y, _Index, -m_Height);
				break;
			case PositionMode.Center:
				upper = new Vector4(_Normal.x, _Normal.y, _Index, m_Height * 0.5f);
				lower = new Vector4(_Normal.x, _Normal.y, _Index, -m_Height * 0.5f);
				break;
			default:
				upper = Vector4.zero;
				lower = Vector4.zero;
				break;
		}
		
		m_UV1.Add(upper);
		m_UV1.Add(upper);
		
		m_UV1.Add(upper);
		m_UV1.Add(upper);
		
		m_UV1.Add(lower);
		m_UV1.Add(lower);
		
		m_UV1.Add(lower);
		m_UV1.Add(lower);
		
		const int quads = 3;
		
		for (int i = 0; i < quads; i++)
		{
			m_Triangles.Add(index + i * 2 + 0);
			m_Triangles.Add(index + i * 2 + 1);
			m_Triangles.Add(index + i * 2 + 2);
			
			m_Triangles.Add(index + i * 2 + 2);
			m_Triangles.Add(index + i * 2 + 1);
			m_Triangles.Add(index + i * 2 + 3);
		}
	}

	void GenerateProperties()
	{
		m_MeshRenderer.GetPropertyBlock(m_PropertyBlock);
		
		Texture2D texture = m_Sprite != null && m_Sprite.texture != null
			? m_Sprite.texture
			: Texture2D.whiteTexture;
		
		m_PropertyBlock.SetTexture(m_MainTexPropertyID, texture);
		
		m_PropertyBlock.SetColor(m_ColorPropertyID, m_Color);
		
		m_MeshRenderer.SetPropertyBlock(m_PropertyBlock);
	}
}