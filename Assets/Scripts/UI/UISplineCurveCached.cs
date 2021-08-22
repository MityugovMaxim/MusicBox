using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UISplineCurveCached : MaskableGraphic
{
	public override Texture mainTexture => m_Sprite != null ? m_Sprite.texture : base.mainTexture;

	[SerializeField, HideInInspector] List<Vector2> m_Positions;
	[SerializeField, HideInInspector] List<Vector2> m_UVs;

	[SerializeField] Sprite m_Sprite;

	readonly List<UIVertex> m_Vertices = new List<UIVertex>();

	protected override void Awake()
	{
		base.Awake();
		
		if (m_Vertices.Count == 0)
			Fill();
	}

	public void Cache(List<UIVertex> _Vertices)
	{
		if (m_Positions == null)
			m_Positions = new List<Vector2>();
		
		if (m_UVs == null)
			m_UVs = new List<Vector2>();
		
		m_Positions.AddRange(_Vertices.Select(_Vertex => (Vector2)_Vertex.position));
		m_UVs.AddRange(_Vertices.Select(_Vertex => _Vertex.uv0));
		
		SetAllDirty();
	}

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		_VertexHelper.Clear();
		
		if (m_Vertices.Count == 0)
			Fill();
		else
			Refresh();
		
		foreach (UIVertex vertex in m_Vertices)
			_VertexHelper.AddVert(vertex);
		
		int quads = m_Vertices.Count / 2 - 1;
		for (int i = 0; i < quads; i++)
		{
			_VertexHelper.AddTriangle(
				i * 2 + 1,
				i * 2 + 0,
				i * 2 + 2
			);
			
			_VertexHelper.AddTriangle(
				i * 2 + 3,
				i * 2 + 1,
				i * 2 + 2
			);
		}
	}

	void Fill()
	{
		Color32 color32 = color;
		
		m_Vertices.Clear();
		
		for (int i = 0; i < m_Positions.Count; i++)
		{
			UIVertex vertex = new UIVertex();
			vertex.position = m_Positions[i];
			vertex.color    = color32;
			vertex.uv0      = m_UVs[i];
			m_Vertices.Add(vertex);
		}
	}

	void Refresh()
	{
		Color32 color32 = color;
		
		for (int i = 0; i < m_Vertices.Count; i++)
		{
			UIVertex vertex = m_Vertices[i];
			vertex.color  = color32;
			m_Vertices[i] = vertex;
		}
	}
}