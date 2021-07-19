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

	[SerializeField] UISpline      m_Spline;
	[SerializeField] Sprite        m_Sprite;
	[SerializeField] RectTransform m_MinCap;
	[SerializeField] RectTransform m_MaxCap;
	[SerializeField] float         m_Size;

	[NonSerialized] UISpline m_SplineCache;

	readonly List<UIVertex> m_Vertices  = new List<UIVertex>();

	#endregion

	#region engine methods

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

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		if (!gameObject.scene.isLoaded)
			return;
		
		SetVerticesDirty();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		
		SetVerticesDirty();
	}

	#endregion

	#region service methods

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		_VertexHelper.Clear();
		
		if (Spline == null)
			return;
		
		m_Vertices.Clear();
		
		if (m_MinCap != null)
		{
			UISpline.Point first = m_Spline.First();
			Vector2 position = rectTransform.TransformPoint(first.Position);
			m_MinCap.position = position;
			m_MinCap.rotation = first.Normal.ToRotation();
		}
		
		if (m_MaxCap != null)
		{
			UISpline.Point last  = m_Spline.Last();
			Vector2 position = rectTransform.TransformPoint(last.Position);
			m_MaxCap.position = position;
			m_MaxCap.rotation = last.Normal.ToRotation(180);
		}
		
		Rect uv = new Rect(0, 0, 1, 1);
		if (Sprite != null && Sprite.texture != null)
		{
			uv        =  Sprite.textureRect;
			uv.x      /= Sprite.texture.width;
			uv.y      /= Sprite.texture.height;
			uv.width  /= Sprite.texture.width;
			uv.height /= Sprite.texture.height;
		}
		
		if (Spline.Loop)
			BuildLoopMesh(uv);
		else
			BuildStraightMesh(uv);
		
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

	void OnSplineRebuild()
	{
		SetVerticesDirty();
	}

	void BuildStraightMesh(Rect _UV)
	{
		if (Spline == null || Spline.Length < 2)
			return;
		
		// process first point
		UISpline.Point firstPoint = Spline.First();
		
		ProcessPoint(firstPoint, _UV, 0);
		
		for (int i = 1; i < Spline.Length - 1; i++)
		{
			UISpline.Point point = Spline[i];
			
			ProcessPoint(point, _UV, point.Phase);
		}
		
		// process last point
		UISpline.Point lastPoint = Spline.Last();
		
		ProcessPoint(lastPoint, _UV, lastPoint.Phase);
	}

	void BuildLoopMesh(Rect _UV)
	{
		if (Spline == null || Spline.Length < 2)
			return;
		
		// process origin point
		UISpline.Point originPoint = Spline[0];
		
		ProcessPoint(originPoint, _UV, 0);
		
		for (int i = 1; i < Spline.Length; i++)
		{
			UISpline.Point point = Spline[i];
			
			ProcessPoint(point, _UV, point.Phase);
		}
		
		ProcessPoint(originPoint, _UV, 1);
	}

	void ProcessPoint(UISpline.Point _Point, Rect _UV, float _Phase)
	{
		float size = m_Size * 0.5f;
		
		UIVertex left = new UIVertex();
		left.position = _Point.Position + _Point.Normal * size;
		left.color    = color;
		left.uv0      = new Vector2(_UV.xMin, _UV.y + _UV.height * _Phase);
		left.tangent  = _UV.ToVector();
		
		UIVertex right = new UIVertex();
		right.position = _Point.Position - _Point.Normal * size;
		right.color    = color;
		right.uv0      = new Vector2(_UV.xMax, _UV.y + _UV.height * _Phase);
		right.tangent  = _UV.ToVector();
		
		m_Vertices.Add(right);
		m_Vertices.Add(left);
	}

	#endregion
}
