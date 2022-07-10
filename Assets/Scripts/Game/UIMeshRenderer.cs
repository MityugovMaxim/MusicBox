using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIMeshRenderer : UIRenderer
{
	public override    int     Thickness   => 1;
	protected abstract Texture MainTexture { get; }
	protected abstract Rect    UV          { get; }

	static readonly int m_MainTexPropertyID = Shader.PropertyToID("_MainTex");

	[SerializeField] ScaleMode  m_ScaleMode  = ScaleMode.Stretch;
	[SerializeField] BorderMode m_BorderMode = BorderMode.Stretch;
	[SerializeField] TextAnchor m_Alignment  = TextAnchor.MiddleCenter;
	[SerializeField] Vector4    m_Border;
	[SerializeField] float      m_BorderScale = 1;
	[SerializeField] bool       m_FillCenter  = true;

	readonly List<Vector3> m_Vertices         = new List<Vector3>();
	readonly List<Vector2> m_UV               = new List<Vector2>();
	readonly List<int>     m_Triangles        = new List<int>();
	readonly List<float>   m_HorizontalVertex = new List<float>();
	readonly List<float>   m_VerticalVertex   = new List<float>();
	readonly List<float>   m_HorizontalUV     = new List<float>();
	readonly List<float>   m_VerticalUV       = new List<float>();

	[NonSerialized] bool m_DirtyMesh = true;

	[NonSerialized] MeshRenderer m_MeshRenderer;
	[NonSerialized] MeshFilter   m_MeshFilter;
	[NonSerialized] Mesh         m_Mesh;

	protected override void LateUpdate()
	{
		base.LateUpdate();
		
		if (m_DirtyMesh)
		{
			m_DirtyMesh = false;
			
			GenerateMesh();
		}
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		
		if (!gameObject.activeInHierarchy)
			return;
		
		InvalidateMesh();
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
		
		if (m_Mesh == null)
			m_Mesh = new Mesh();
		
		InvalidateMesh();
	}
	#endif

	protected override Renderer CreateRenderer()
	{
		m_MeshRenderer = gameObject.GetComponent<MeshRenderer>();
		if (m_MeshRenderer == null)
			m_MeshRenderer = gameObject.AddComponent<MeshRenderer>();
		
		m_MeshFilter = gameObject.GetComponent<MeshFilter>();
		if (m_MeshFilter == null)
			m_MeshFilter = gameObject.AddComponent<MeshFilter>();
		
		m_Mesh = new Mesh();
		
		return m_MeshRenderer;
	}

	protected void InvalidateMesh()
	{
		m_DirtyMesh = true;
	}

	void GenerateMesh()
	{
		Rect rect = GetLocalRect();
		
		Vector2 size = Vector2.Scale(new Vector2(MainTexture.width, MainTexture.height), UV.size);
		
		Vector2 pivot = m_Alignment.GetPivot();
		
		float aspect = !Mathf.Approximately(size.y, 0) ? size.x / size.y : 1;
		
		Rect uv = new Rect(0, 0, 1, 1);
		
		switch (m_ScaleMode)
		{
			case ScaleMode.Fit:
				rect = MathUtility.Fit(rect, aspect, pivot);
				break;
			case ScaleMode.Fill:
				rect = MathUtility.Fill(rect, aspect, pivot);
				break;
			case ScaleMode.Crop:
				uv = MathUtility.Fit(uv, rect.width / rect.height, pivot);
				break;
		}
		
		m_Vertices.Clear();
		m_UV.Clear();
		m_Triangles.Clear();
		
		Vector4 vertexBorder = GetVertexBorder(rect);
		Vector4 uvBorder     = GetUVBorder(size.x, size.y);
		
		m_HorizontalVertex.Clear();
		m_VerticalVertex.Clear();
		
		m_HorizontalUV.Clear();
		m_VerticalUV.Clear();
		
		m_HorizontalVertex.Add(rect.xMin);
		m_VerticalVertex.Add(rect.yMax);
		
		m_HorizontalUV.Add(uv.xMin);
		m_VerticalUV.Add(uv.yMax);
		
		if (m_ScaleMode == ScaleMode.Stretch)
		{
			if (vertexBorder.x > float.Epsilon)
			{
				m_HorizontalVertex.Add(rect.xMin + vertexBorder.x);
				m_HorizontalUV.Add(uv.xMin + uvBorder.x);
			}
			
			if (vertexBorder.y > float.Epsilon)
			{
				m_HorizontalVertex.Add(rect.xMax - vertexBorder.y);
				m_HorizontalUV.Add(uv.xMax - uvBorder.y);
			}
			
			if (vertexBorder.z > float.Epsilon)
			{
				m_VerticalVertex.Add(rect.yMax - vertexBorder.z);
				m_VerticalUV.Add(uv.yMax - uvBorder.z);
			}
			
			if (vertexBorder.w > float.Epsilon)
			{
				m_VerticalVertex.Add(rect.yMin + vertexBorder.w);
				m_VerticalUV.Add(uv.yMin + uvBorder.w);
			}
		}
		
		m_HorizontalVertex.Add(rect.xMax);
		m_VerticalVertex.Add(rect.yMin);
		
		m_HorizontalUV.Add(uv.xMax);
		m_VerticalUV.Add(uv.yMin);
		
		int hCenter = (m_HorizontalVertex.Count - 1) / 2;
		int vCenter = (m_VerticalVertex.Count - 1) / 2;
		
		int position = m_Vertices.Count;
		
		for (int y = 0; y < m_VerticalVertex.Count - 1; y++)
		for (int x = 0; x < m_HorizontalVertex.Count - 1; x++)
		{
			int index = position + y * m_HorizontalVertex.Count + x;
			
			if (!m_FillCenter && x == hCenter && y == vCenter)
				continue;
			
			m_Triangles.Add(index);
			m_Triangles.Add(index + 1);
			m_Triangles.Add(index + m_HorizontalVertex.Count);
			
			m_Triangles.Add(index + m_HorizontalVertex.Count);
			m_Triangles.Add(index + 1);
			m_Triangles.Add(index + m_HorizontalVertex.Count + 1);
		}
		
		FillUV(UV, m_UV);
		
		foreach (float y in m_VerticalVertex)
		foreach (float x in m_HorizontalVertex)
			m_Vertices.Add(new Vector3(x, y));
		
		m_Mesh.Clear();
		m_Mesh.SetVertices(m_Vertices);
		m_Mesh.SetUVs(0, m_UV);
		m_Mesh.SetTriangles(m_Triangles, 0);
		
		FillMesh(m_Mesh);
		
		m_Mesh.RecalculateNormals();
		m_Mesh.RecalculateBounds();
		
		m_MeshFilter.sharedMesh = m_Mesh;
	}

	protected override void FillProperty(MaterialPropertyBlock _PropertyBlock)
	{
		Texture texture = MainTexture != null ? MainTexture : Texture2D.whiteTexture;
		
		_PropertyBlock.SetTexture(m_MainTexPropertyID, texture);
	}

	Vector4 GetVertexBorder(Rect _Rect)
	{
		float width  = Mathf.Abs(_Rect.width);
		float height = Mathf.Abs(_Rect.height);
		
		Vector4 border;
		switch (m_BorderMode)
		{
			case BorderMode.Fit:
				float hFit = m_Border.x + m_Border.y;
				float vFit = m_Border.z + m_Border.w;
				
				if (hFit + vFit < float.Epsilon)
				{
					border = m_Border;
					break;
				}
				
				float fit = Mathf.Min(
					width / (hFit > float.Epsilon ? hFit : vFit),
					height / (vFit > float.Epsilon ? vFit : hFit)
				);
				
				border = m_Border * fit;
				break;
			
			case BorderMode.Fill:
				float hFill = m_Border.x + m_Border.x;
				float vFill = m_Border.z + m_Border.w;
				
				if (hFill + vFill < float.Epsilon)
				{
					border = m_Border;
					break;
				}
				
				float fill = Mathf.Max(
					width / (hFill > float.Epsilon ? hFill : vFill),
					height / (vFill > float.Epsilon ? vFill : hFill)
				);
				border = m_Border * fill;
				break;
			
			default:
				border = m_Border;
				break;
		}
		
		border *= m_BorderScale;
		
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

	Vector4 GetUVBorder(float _Width, float _Height)
	{
		return new Vector4(
			m_Border.x / _Width,
			m_Border.y / _Width,
			m_Border.z / _Height,
			m_Border.w / _Height
		);
	}

	protected void FillUV(Rect _Rect, List<Vector2> _UV)
	{
		_UV.Clear();
		
		for (int y = 0; y < m_VerticalVertex.Count; y++)
		for (int x = 0; x < m_HorizontalVertex.Count; x++)
			_UV.Add(new Vector2(_Rect.x + m_HorizontalUV[x] * _Rect.width, _Rect.y + m_VerticalUV[y] * _Rect.height));
	}

	protected virtual void FillMesh(Mesh _Mesh) { }
}