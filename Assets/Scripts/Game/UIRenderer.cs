using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class UIRenderer : UIEntity
{
	static readonly int m_MainTexPropertyID = Shader.PropertyToID("_MainTex");
	static readonly int m_ColorPropertyID   = Shader.PropertyToID("_Color");

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

	public float Alpha
	{
		get => m_Color.a;
		set
		{
			if (Mathf.Approximately(m_Color.a, value))
				return;
			
			m_Color.a = value;
			
			InvalidateProperties();
		}
	}

	[SerializeField] Color      m_Color = Color.white;
	[SerializeField] Sprite     m_Sprite;
	[SerializeField] ScaleMode  m_ScaleMode  = ScaleMode.Stretch;
	[SerializeField] BorderMode m_BorderMode = BorderMode.Stretch;
	[SerializeField] TextAnchor m_Alignment  = TextAnchor.MiddleCenter;
	[SerializeField] Vector4    m_Border;
	[SerializeField] float      m_BorderScale = 1;
	[SerializeField] bool       m_FillCenter;

	MeshFilter            m_MeshFilter;
	MeshRenderer          m_MeshRenderer;
	Mesh                  m_Mesh;
	MaterialPropertyBlock m_PropertyBlock;

	Rect m_RectCache;

	bool m_DirtyMesh;
	bool m_DirtyProperties;

	readonly List<Vector3> m_Vertices         = new List<Vector3>();
	readonly List<Vector2> m_UV               = new List<Vector2>();
	readonly List<int>     m_Triangles        = new List<int>();
	readonly List<float>   m_HorizontalVertex = new List<float>();
	readonly List<float>   m_VerticalVertex   = new List<float>();
	readonly List<float>   m_HorizontalUV     = new List<float>();
	readonly List<float>   m_VerticalUV       = new List<float>();

	protected override void Awake()
	{
		base.Awake();
		
		m_MeshFilter    = GetComponent<MeshFilter>();
		m_MeshRenderer  = GetComponent<MeshRenderer>();
		m_PropertyBlock = new MaterialPropertyBlock();
		m_Mesh          = new Mesh();
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
			
			GenerateProperty();
		}
	}

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		GenerateProperty();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (Application.isPlaying)
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
		
		GenerateProperty();
	}
	#endif

	protected override void OnEnable()
	{
		base.OnEnable();
		
		InvalidateMesh();
		
		InvalidateProperties();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		
		if (!gameObject.activeInHierarchy)
			return;
		
		GenerateMesh();
	}

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
		Rect rect = GetLocalRect();
		
		Rect sprite = m_Sprite != null ? m_Sprite.textureRect : rect;
		
		Vector2 pivot = m_Alignment.GetPivot();
		
		float aspect = !Mathf.Approximately(sprite.height, 0) ? sprite.width / sprite.height : 1;
		
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
		Vector4 uvBorder     = GetUVBorder(sprite.width, sprite.height);
		
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
		
		FillUV(m_Sprite, m_UV);
		
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

	void GenerateProperty()
	{
		m_MeshRenderer.GetPropertyBlock(m_PropertyBlock);
		
		Texture2D texture = m_Sprite != null && m_Sprite.texture != null
			? m_Sprite.texture
			: Texture2D.whiteTexture;
		
		m_PropertyBlock.SetTexture(m_MainTexPropertyID, texture);
		
		m_PropertyBlock.SetColor(m_ColorPropertyID, m_Color);
		
		FillProperty(m_PropertyBlock);
		
		m_MeshRenderer.SetPropertyBlock(m_PropertyBlock);
	}

	protected Rect GetUV(Sprite _Sprite)
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

	protected void FillUV(Sprite _Sprite, List<Vector2> _UV)
	{
		FillUV(GetUV(_Sprite), _UV);
	}

	protected void FillUV(Rect _Rect, List<Vector2> _UV)
	{
		_UV.Clear();
		
		for (int y = 0; y < m_VerticalVertex.Count; y++)
		for (int x = 0; x < m_HorizontalVertex.Count; x++)
			_UV.Add(new Vector2(_Rect.x + m_HorizontalUV[x] * _Rect.width, _Rect.y + m_VerticalUV[y] * _Rect.height));
	}

	protected virtual void FillMesh(Mesh _Mesh) { }

	protected virtual void FillProperty(MaterialPropertyBlock _PropertyBlock) { }
}