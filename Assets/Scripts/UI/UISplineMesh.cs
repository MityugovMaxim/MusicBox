using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISplineMesh : MaskableGraphic
{
	[SerializeField] UISpline m_Spline;

	UISpline m_SplineCache;

	MeshBuilder m_MeshBuilder;

	readonly List<Vector2> m_Vertices  = new List<Vector2>();
	readonly List<int>     m_Triangles = new List<int>();

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (!gameObject.scene.isLoaded)
			return;
		
		if (m_SplineCache != m_Spline)
		{
			if (m_SplineCache != null)
				m_SplineCache.OnRebuild -= SetAllDirty;
			
			m_SplineCache = m_Spline;
			
			if (m_SplineCache != null)
				m_SplineCache.OnRebuild += SetAllDirty;
		}
	}
	#endif

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		base.OnPopulateMesh(_VertexHelper);
		
		if (m_MeshBuilder == null)
			m_MeshBuilder = new MeshBuilder(m_Vertices);
		
		m_Vertices.Clear();
		m_Triangles.Clear();
		
		_VertexHelper.Clear();
		
		foreach (UISpline.Point point in m_Spline)
			m_Vertices.Add(point.Position);
		
		m_MeshBuilder.Triangulate(m_Triangles);
		
		foreach (Vector2 vertex in m_Vertices)
			_VertexHelper.AddVert(vertex, color, Vector2.zero);
		
		for (int i = 0; i < m_Triangles.Count; i += 3)
		{
			_VertexHelper.AddTriangle(
				m_Triangles[i + 0],
				m_Triangles[i + 1],
				m_Triangles[i + 2]
			);
		}
	}
}