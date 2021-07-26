using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UISplineProgress : MaskableGraphic
{
	public enum RenderingMode
	{
		Blend,
		Additive
	}

	public float Min
	{
		get => m_Min;
		set
		{
			if (Mathf.Approximately(m_Min, value))
				return;
			
			m_Min = value;
			
			SetVerticesDirty();
		}
	}

	public float Max
	{
		get => m_Max;
		set
		{
			if (Mathf.Approximately(m_Max, value))
				return;
			
			m_Max = value;
			
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

	public override Material defaultMaterial
	{
		get
		{
			if (m_RenderingMode == RenderingMode.Blend)
			{
				if (m_BlendMaterial == null)
				{
					m_BlendMaterial = new Material(Shader.Find("UI/Spline/Default"));
					m_BlendMaterial.SetInt(m_SrcBlend, (int)BlendMode.SrcAlpha);
					m_BlendMaterial.SetInt(m_DstBlend, (int)BlendMode.OneMinusSrcAlpha);
				}
				return m_BlendMaterial;
			}
			else
			{
				if (m_AdditiveMaterial == null)
				{
					m_AdditiveMaterial = new Material(Shader.Find("UI/Spline/Default"));
					m_AdditiveMaterial.SetInt(m_SrcBlend, (int)BlendMode.SrcAlpha);
					m_AdditiveMaterial.SetInt(m_DstBlend, (int)BlendMode.One);
				}
				return m_AdditiveMaterial;
			}
		}
	}

	public override Texture mainTexture => m_Sprite != null ? m_Sprite.texture : base.mainTexture;

	static readonly int m_SrcBlend = Shader.PropertyToID("_SrcBlend");
	static readonly int m_DstBlend = Shader.PropertyToID("_DstBlend");

	static Material m_BlendMaterial;
	static Material m_AdditiveMaterial;

	[SerializeField]              UISpline      m_Spline;
	[SerializeField]              Sprite        m_Sprite;
	[SerializeField]              Graphic       m_StartCap;
	[SerializeField]              Graphic       m_EndCap;
	[SerializeField]              float         m_Size;
	[SerializeField]              RenderingMode m_RenderingMode;
	[SerializeField, Range(0, 1)] float         m_Min = 0;
	[SerializeField, Range(0, 1)] float         m_Max = 1;
	[SerializeField]              float         m_FadeIn;
	[SerializeField]              float         m_FadeOut;
	[SerializeField]              float         m_Offset;
	[SerializeField]              bool          m_AbsoluteFade;

	readonly List<UIVertex> m_Vertices = new List<UIVertex>();

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		_VertexHelper.Clear();
		
		if (m_Spline == null)
			return;
		
		float min = Min + Offset;
		float max = Max + Offset;
		
		UISpline.Point first = m_Spline.GetPoint(min);
		UISpline.Point last  = m_Spline.GetPoint(max);
		
		if (m_StartCap != null)
		{
			Vector2 position = rectTransform.TransformPoint(first.Position);
			m_StartCap.rectTransform.position = position;
			m_StartCap.rectTransform.rotation = first.Normal.ToRotation();
		}
		
		if (m_EndCap != null)
		{
			Vector2 position = rectTransform.TransformPoint(last.Position);
			m_EndCap.rectTransform.position = position;
			m_EndCap.rectTransform.rotation = last.Normal.ToRotation(180);
		}
		
		if (Mathf.Approximately(Min, Max) || Min >= Max)
			return;
		
		Rect uv = new Rect(0, 0, 1, 1);
		if (m_Sprite != null && m_Sprite.texture != null)
		{
			uv        =  m_Sprite.textureRect;
			uv.x      /= m_Sprite.texture.width;
			uv.y      /= m_Sprite.texture.height;
			uv.width  /= m_Sprite.texture.width;
			uv.height /= m_Sprite.texture.height;
		}
		
		m_Vertices.Clear();
		
		ProcessPoint(first, min, max, uv);
		
		foreach (UISpline.Point point in m_Spline)
		{
			if (point.Phase > first.Phase && point.Phase < last.Phase)
				ProcessPoint(point, min, max, uv);
		}
		
		ProcessPoint(last, min, max, uv);
		
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

	void ProcessPoint(UISpline.Point _Point, float _Min, float _Max, Rect _UV)
	{
		float size    = m_Size * 0.5f;
		float phase   = Mathf.InverseLerp(_Min, _Max, _Point.Phase);
		float length  = m_Spline.GetLength(1);
		float fadeIn  = m_AbsoluteFade ? m_FadeIn / length : m_FadeIn;
		float fadeOut = m_AbsoluteFade ? m_FadeOut / length : m_FadeOut;
		
		UIVertex left  = new UIVertex();
		left.position = _Point.Position + _Point.Normal * size;
		left.color    = color;
		left.uv0      = new Vector2(0, phase);
		left.uv1      = new Vector2(fadeIn, fadeOut);
		left.tangent  = _UV.ToVector();
		left.normal   = new Vector4(_Min, _Max, _Point.Phase, _Point.Phase);
		
		UIVertex right = new UIVertex();
		right.position = _Point.Position - _Point.Normal * size;
		right.color    = color;
		right.uv0      = new Vector2(1, phase);
		right.uv1      = new Vector2(fadeIn, fadeOut);
		right.tangent  = _UV.ToVector();
		right.normal   = new Vector4(_Min, _Max, _Point.Phase, _Point.Phase);
		
		m_Vertices.Add(right);
		m_Vertices.Add(left);
	}
}