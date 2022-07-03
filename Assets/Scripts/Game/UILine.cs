using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class UILine : UIEntity
{
	public Color Color
	{
		get => m_Color;
		set
		{
			if (m_Color == value)
				return;
			
			m_Color = value;
			
			InvalidateProperties();
		}
	}

	public float Min
	{
		get => m_MinProgress;
		set
		{
			if (Mathf.Approximately(m_MinProgress, value))
				return;
			
			m_MinProgress = value;
			
			InvalidateMesh();
		}
	}

	public float Max
	{
		get => m_MaxProgress;
		set
		{
			if (Mathf.Approximately(m_MinProgress, value))
				return;
			
			m_MinProgress = value;
			
			InvalidateMesh();
		}
	}

	static readonly int m_MainTexPropertyID = Shader.PropertyToID("_MainTex");
	static readonly int m_ColorPropertyID   = Shader.PropertyToID("_Color");

	[SerializeField]              Color    m_Color = Color.white;
	[SerializeField]              UISpline m_Spline;
	[SerializeField]              Sprite   m_Sprite;
	[SerializeField]              float    m_Width;
	[SerializeField, Range(0, 1)] float    m_MinProgress = 0;
	[SerializeField, Range(0, 1)] float    m_MaxProgress = 1;

	readonly List<Vector3> m_Vertices  = new List<Vector3>();
	readonly List<Vector2> m_UV        = new List<Vector2>();
	readonly List<int>     m_Triangles = new List<int>();

	bool m_DirtyMesh       = true;
	bool m_DirtyProperties = true;

	MeshFilter            m_MeshFilter;
	MeshRenderer          m_MeshRenderer;
	MaterialPropertyBlock m_PropertyBlock;
	Mesh                  m_Mesh;

	protected override void Awake()
	{
		base.Awake();
		
		m_MeshRenderer  = GetComponent<MeshRenderer>();
		m_MeshFilter    = GetComponent<MeshFilter>();
		m_PropertyBlock = new MaterialPropertyBlock();
		m_Mesh          = new Mesh();
		
		m_Spline.OnRebuild += InvalidateMesh;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Spline.OnRebuild -= InvalidateMesh;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		InvalidateMesh();
		
		InvalidateProperties();
	}

	void LateUpdate()
	{
		if (m_DirtyMesh)
		{
			m_DirtyMesh = false;
			
			GenerateMesh();
		}
		
		if (m_DirtyProperties)
		{
			m_DirtyProperties = false;
			
			GenerateProperties();
		}
	}

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		GenerateProperties();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
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

	void InvalidateMesh()
	{
		m_DirtyMesh = true;
	}

	void InvalidateProperties()
	{
		m_DirtyProperties = true;
	}

	void GenerateMesh()
	{
		// 0 -- 1
		// |    |
		// 2 -- 3
		// |    |
		// 2 -- 4
		
		m_Vertices.Clear();
		m_UV.Clear();
		m_Triangles.Clear();
		
		float size = m_Width * 0.5f;
		
		Rect uv = MeshUtility.GetUV(m_Sprite);
		
		float   centerUV = Mathf.Lerp(uv.yMin, uv.yMax, 0.5f);
		Vector2 minUV    = new Vector2(uv.xMin, centerUV);
		Vector2 maxUV    = new Vector2(uv.xMax, centerUV);

		void AddPoint(UISpline.Point _Point)
		{
			Vector2 width = _Point.Normal * size;
			m_Vertices.Add(_Point.Position + width);
			m_Vertices.Add(_Point.Position - width);
			
			m_UV.Add(minUV);
			m_UV.Add(maxUV);
		}
		
		// Min Cap
		GenerateMinCap(uv);
		
		UISpline.Point a = m_Spline.GetPoint(m_MinProgress);
		AddPoint(a);
		
		foreach (UISpline.Point point in m_Spline)
		{
			if (point.Phase < m_MinProgress || point.Phase > m_MaxProgress)
				continue;
			
			AddPoint(point);
		}
		
		UISpline.Point b = m_Spline.GetPoint(m_MaxProgress);
		AddPoint(b);
		
		GenerateMaxCap(uv);
		
		int quads = m_Vertices.Count / 2 - 1;
		for (int i = 0; i < quads; i++)
		{
			m_Triangles.Add(i * 2 + 0);
			m_Triangles.Add(i * 2 + 1);
			m_Triangles.Add(i * 2 + 2);
			
			m_Triangles.Add(i * 2 + 2);
			m_Triangles.Add(i * 2 + 1);
			m_Triangles.Add(i * 2 + 3);
		}
		
		m_Mesh.Clear();
		m_Mesh.SetVertices(m_Vertices);
		m_Mesh.SetUVs(0, m_UV);
		m_Mesh.SetTriangles(m_Triangles, 0);
		
		m_Mesh.RecalculateBounds();
		m_Mesh.RecalculateNormals();
		
		m_MeshFilter.sharedMesh = m_Mesh;
	}

	void GenerateMinCap(Rect _UV)
	{
		float height = m_Width / (_UV.width / _UV.height) * 0.5f;
		
		UISpline.Point point = m_Spline.GetPoint(m_MinProgress);
		
		Vector2 width  = point.Normal * m_Width * 0.5f;
		Vector2 normal = point.Normal.Rotate90();
		
		m_Vertices.Add(point.Position + width - normal * height);
		m_Vertices.Add(point.Position - width - normal * height);
		
		m_UV.Add(new Vector2(_UV.xMin, _UV.yMin));
		m_UV.Add(new Vector2(_UV.xMax, _UV.yMin));
	}

	void GenerateMaxCap(Rect _UV)
	{
		float height = m_Width / (_UV.width / _UV.height) * 0.5f;
		
		UISpline.Point point = m_Spline.GetPoint(m_MaxProgress);
		
		Vector2 width  = point.Normal * m_Width * 0.5f;
		Vector2 normal = point.Normal.Rotate90();
		
		m_Vertices.Add(point.Position + width + normal * height);
		m_Vertices.Add(point.Position - width + normal * height);
		
		m_UV.Add(new Vector2(_UV.xMin, _UV.yMax));
		m_UV.Add(new Vector2(_UV.xMax, _UV.yMax));
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