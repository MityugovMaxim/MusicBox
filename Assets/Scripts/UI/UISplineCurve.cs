using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UISplineCurve : MaskableGraphic
{
	#region constants

	const float MAX_SIZE = 3;

	#endregion

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

	public float Size
	{
		get => m_Size;
		set
		{
			if (Mathf.Approximately(m_Size, value))
				return;
			
			m_Size = value;
			
			SetVerticesDirty();
		}
	}

	public float SpriteOffset
	{
		get { return m_SpriteOffset; }
		set
		{
			if (Mathf.Approximately(m_SpriteOffset, value))
				return;
			
			m_SpriteOffset = value;
			
			SetVerticesDirty();
		}
	}

	public float SpriteScale
	{
		get { return m_SpriteScale; }
		set
		{
			if (Mathf.Approximately(m_SpriteScale, value))
				return;
			
			m_SpriteScale = value;
			
			SetVerticesDirty();
		}
	}

	public Gradient Gradient
	{
		get { return m_Gradient; }
		set
		{
			m_Gradient = value;
			
			SetVerticesDirty();
		}
	}

	#endregion

	#region attributes

	[SerializeField] UISpline m_Spline;
	[SerializeField] Sprite   m_Sprite;
	[SerializeField] float    m_Size;
	[SerializeField] float    m_SpriteOffset;
	[SerializeField] float    m_SpriteScale;
	[SerializeField] Gradient m_Gradient;

	[NonSerialized] UISpline m_SplineCache;

	readonly List<Vector3> m_Vertices  = new List<Vector3>();
	readonly List<int>     m_Triangles = new List<int>();
	readonly List<Vector2> m_UV        = new List<Vector2>();
	readonly List<Color>   m_Colors    = new List<Color>();

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
		m_UV.Clear();
		m_Colors.Clear();
		m_Triangles.Clear();
		
		if (Spline.Loop)
			BuildLoopMesh();
		else
			BuildStraightMesh();
		
		for (int i = 0; i < m_Vertices.Count; i++)
		{
			_VertexHelper.AddVert(
				m_Vertices[i],
				m_Colors[i],
				m_UV[i]
			);
		}
		
		for (int i = 0; i < m_Triangles.Count; i += 3)
		{
			_VertexHelper.AddTriangle(
				m_Triangles[i + 2],
				m_Triangles[i + 1],
				m_Triangles[i + 0]
			);
		}
	}

	void OnSplineRebuild()
	{
		SetVerticesDirty();
	}

	void BuildStraightMesh()
	{
		if (Spline == null || Spline.Length < 2)
			return;
		
		// process first point
		UISpline.Point firstPoint = Spline.First();
		
		ProcessVertex(firstPoint.Position, firstPoint.Normal, Size);
		ProcessUV(0);
		ProcessColor(0);
		
		for (int i = 1; i < Spline.Length - 1; i++)
		{
			UISpline.Point point = Spline[i];
			
			Vector2 direction = Spline[i - 1].Position - point.Position;
			
			float angle = Vector2.SignedAngle(direction, point.Normal);
			
			float length = Size / Mathf.Sin(angle * Mathf.Deg2Rad);
			
			ProcessVertex(point.Position, point.Normal, length);
			ProcessUV(point.Phase);
			ProcessColor(point.Phase);
		}
		
		// process last point
		UISpline.Point lastPoint = Spline.Last();
		
		ProcessVertex(lastPoint.Position, lastPoint.Normal, Size);
		ProcessUV(1);
		ProcessColor(1);
		
		ProcessTriangles(Spline.Length - 1);
	}

	void BuildLoopMesh()
	{
		if (Spline == null || Spline.Length < 2)
			return;
		
		// process origin point
		UISpline.Point originPoint     = Spline[0];
		Vector2        originDirection = Spline[Spline.Length - 1].Position - originPoint.Position;
		float          originAngle     = Vector2.SignedAngle(originDirection, originPoint.Normal);
		float          originLength    = Size / Mathf.Sin(originAngle * Mathf.Deg2Rad);
		
		ProcessVertex(originPoint.Position, originPoint.Normal, originLength);
		ProcessUV(0);
		ProcessColor(0);
		
		for (int i = 1; i < Spline.Length; i++)
		{
			UISpline.Point point = Spline[i];
			
			Vector2 direction = Spline[i - 1].Position - point.Position;
			
			float angle = Vector2.SignedAngle(direction, point.Normal);
			
			float length = Size / Mathf.Sin(angle * Mathf.Deg2Rad);
			
			ProcessVertex(point.Position, point.Normal, length);
			ProcessUV(point.Phase);
			ProcessColor(point.Phase);
		}
		
		ProcessVertex(originPoint.Position, originPoint.Normal, originLength);
		ProcessUV(1);
		ProcessColor(1);
		
		ProcessTriangles(Spline.Length);
	}

	void ProcessVertex(Vector2 _Position, Vector2 _Normal, float _Length)
	{
		float length = Mathf.Clamp(_Length, Size, Size * MAX_SIZE);
		
		Vector2 point = _Normal * length;
		
		m_Vertices.Add(_Position + point);
		m_Vertices.Add(_Position - point);
	}

	void ProcessUV(float _Phase)
	{
		float phase = _Phase - SpriteOffset;
		
		if (Sprite == null || Sprite.texture == null)
		{
			m_UV.Add(new Vector2(0, phase));
			m_UV.Add(new Vector2(1, phase));
			return;
		}
		
		Rect uv = Sprite.textureRect;
		
		uv.height /= SpriteScale;
		
		uv.x      /= Sprite.texture.width;
		uv.y      /= Sprite.texture.height;
		uv.width  /= Sprite.texture.width;
		uv.height /= Sprite.texture.height;
		
		float value = uv.y + uv.height * phase;
		
		m_UV.Add(new Vector2(uv.xMin, value));
		m_UV.Add(new Vector2(uv.xMax, value));
	}

	void ProcessColor(float _Phase)
	{
		Color color = m_Gradient.Evaluate(_Phase) * base.color;
		
		m_Colors.Add(color);
		m_Colors.Add(color);
	}

	void ProcessTriangles(int _Quads)
	{
		for (int i = 0; i < _Quads; i++)
		{
			m_Triangles.Add(i * 2 + 1);
			m_Triangles.Add(i * 2 + 0);
			m_Triangles.Add(i * 2 + 2);
			
			m_Triangles.Add(i * 2 + 3);
			m_Triangles.Add(i * 2 + 1);
			m_Triangles.Add(i * 2 + 2);
		}
	}

	#endregion
}
