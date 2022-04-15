using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UISplineCurve : MaskableGraphic
{
	#region properties

	public UISpline Spline
	{
		get => m_Spline;
		set
		{
			if (m_Spline == value)
				return;
			
			if (m_Spline != null)
				m_Spline.OnRebuild -= OnSplineRebuild;
			
			m_Spline      = value;
			m_SplineCache = value;
			
			if (m_Spline != null)
				m_Spline.OnRebuild += OnSplineRebuild;
			
			SetVerticesDirty();
		}
	}

	public float Offset
	{
		get => m_Offset;
		set
		{
			if (Mathf.Approximately(m_Offset, value))
				return;
			
			m_Offset = value;
			
			SetVerticesDirty();
		}
	}

	public Sprite Sprite
	{
		get => m_Sprite;
		set
		{
			if (m_Sprite == value)
				return;
			
			m_Sprite = value;
			
			SetMaterialDirty();
			SetVerticesDirty();
		}
	}

	public override Texture mainTexture => Sprite != null ? Sprite.texture : base.mainTexture;

	#endregion

	#region attributes

	static Material m_BlendMaterial;
	static Material m_AdditiveMaterial;

	[SerializeField] bool          m_Outer;
	[SerializeField] UISpline      m_Spline;
	[SerializeField] Sprite        m_Sprite;
	[SerializeField] RectTransform m_MinCap;
	[SerializeField] RectTransform m_MaxCap;
	[SerializeField] float         m_Size;
	[SerializeField] float         m_Offset;
	[SerializeField] float         m_Scale;

	[NonSerialized] UISpline m_SplineCache;

	readonly List<UIVertex> m_Vertices = new List<UIVertex>();
	readonly List<int>      m_Indices  = new List<int>();

	#endregion

	#region engine methods

	[ContextMenu("Cache")]
	void Cache()
	{
		UISplineCurveCached curve = GetComponentInChildren<UISplineCurveCached>();
		
		if (curve == null)
			return;
		
		curve.Cache(m_Vertices);
	}

	protected override void Awake()
	{
		base.Awake();
		
		if (Spline != null)
			Spline.OnRebuild += OnSplineRebuild;
		
		SetVerticesDirty();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		if (Spline != null)
			Spline.OnRebuild -= OnSplineRebuild;
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (!gameObject.scene.isLoaded)
			return;
		
		if (Spline != m_SplineCache)
		{
			if (m_SplineCache != null)
				m_SplineCache.OnRebuild -= OnSplineRebuild;
			
			m_SplineCache = Spline;
			
			if (m_SplineCache != null)
				m_SplineCache.OnRebuild += OnSplineRebuild;
		}
		
		SetAllDirty();
	}
	#endif

	#endregion

	#region service methods

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		_VertexHelper.Clear();
		
		if (Spline == null || Spline.Length < 2)
			return;
		
		m_Vertices.Clear();
		m_Indices.Clear();
		
		ProcessCaps();
		
		Rect uv = new Rect(0, 0, 1, 1);
		if (Sprite != null && Sprite.texture != null)
		{
			float width  = Sprite.texture.width;
			float height = Sprite.texture.height;
			uv        =  Sprite.textureRect;
			uv.x      /= width;
			uv.y      /= height;
			uv.width  /= width;
			uv.height /= height;
		}
		
		foreach (UISpline.Point point in Spline)
			ProcessPoint(point, uv, point.Phase);
		
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

	void ProcessCaps()
	{
		if (m_MinCap != null)
		{
			UISpline.Point first    = m_Spline.First();
			Vector2        position = rectTransform.TransformPoint(first.Position);
			m_MinCap.position = position;
			m_MinCap.rotation = first.Normal.ToRotation();
		}
		
		if (m_MaxCap != null)
		{
			UISpline.Point last     = m_Spline.Last();
			Vector2        position = rectTransform.TransformPoint(last.Position);
			m_MaxCap.position = position;
			m_MaxCap.rotation = last.Normal.ToRotation(180);
		}
	}

	void OnSplineRebuild()
	{
		SetVerticesDirty();
	}

	void ProcessPoint(UISpline.Point _Point, Rect _UV, float _Phase)
	{
		float width = m_Size * 0.5f;
		
		Vector2 size = new Vector2(m_Spline.GetLength(1), m_Size);
		
		UIVertex left = new UIVertex();
		left.position = _Point.Position + _Point.Normal * width * (m_Outer ? 0 : 1);
		left.color    = color;
		left.uv0      = new Vector2(_UV.xMin, _UV.y + _UV.height * _Phase * m_Scale + m_Offset);
		left.uv1      = new Vector2(_Phase, 0);
		left.uv2      = size;
		left.tangent  = _UV.ToVector();
		
		UIVertex right = new UIVertex();
		right.position = _Point.Position - _Point.Normal * width;
		right.color    = color;
		right.uv0      = new Vector2(_UV.xMax, _UV.y + _UV.height * _Phase * m_Scale + m_Offset);
		right.uv1      = new Vector2(_Phase, 1);
		right.uv2      = size;
		right.tangent  = _UV.ToVector();
		
		m_Vertices.Add(right);
		m_Vertices.Add(left);
	}

	#endregion
}
