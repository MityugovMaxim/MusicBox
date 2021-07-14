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
					m_BlendMaterial = new Material(Shader.Find("UI/Progress"));
					m_BlendMaterial.SetInt(m_SrcBlend, (int)BlendMode.SrcAlpha);
					m_BlendMaterial.SetInt(m_DstBlend, (int)BlendMode.OneMinusSrcAlpha);
				}
				return m_BlendMaterial;
			}
			else
			{
				if (m_AdditiveMaterial == null)
				{
					m_AdditiveMaterial = new Material(Shader.Find("UI/Progress"));
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

	[SerializeField]              UISpline       m_Spline;
	[SerializeField]              float          m_Size;
	[SerializeField]              RenderingMode  m_RenderingMode;
	[SerializeField, Range(0, 1)] float          m_Min = 0;
	[SerializeField, Range(0, 1)] float          m_Max = 1;
	[SerializeField, Range(0, 1)] float          m_FadeIn;
	[SerializeField, Range(0, 1)] float          m_FadeOut;
	[SerializeField]              float          m_Offset;
	[SerializeField]              Sprite         m_Sprite;

	readonly List<UIVertex> m_Vertices = new List<UIVertex>();

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		_VertexHelper.Clear();
		
		if (m_Spline == null || Mathf.Approximately(m_Min, m_Max) || m_Min >= m_Max)
			return;
		
		float min = m_Min + m_Offset;
		float max = m_Max + m_Offset;
		
		UISpline.Point first = m_Spline.GetPoint(min);
		UISpline.Point last  = m_Spline.GetPoint(max);
		
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
		float size  = m_Size * 0.5f;
		float phase = Mathf.InverseLerp(_Min, _Max, _Point.Phase);
		
		UIVertex left  = new UIVertex();
		left.position = _Point.Position + _Point.Normal * size;
		left.color    = color;
		left.uv0      = new Vector2(_UV.xMin, _UV.y + _UV.height * phase);
		left.uv1      = new Vector2(m_FadeIn, m_FadeOut);
		left.uv2      = new Vector2(0, phase * 3);
		left.tangent  = _UV.ToVector();
		left.normal   = new Vector4(_Min, _Max, _Point.Phase, _Point.Phase);
		
		UIVertex right = new UIVertex();
		right.position = _Point.Position - _Point.Normal * size;
		right.color    = color;
		right.uv0      = new Vector2(_UV.xMax, _UV.y + _UV.height * phase);
		right.uv1      = new Vector2(m_FadeIn, m_FadeOut);
		right.uv2      = new Vector2(1, phase * 3);
		right.tangent  = _UV.ToVector();
		right.normal   = new Vector4(_Min, _Max, _Point.Phase, _Point.Phase);
		
		m_Vertices.Add(right);
		m_Vertices.Add(left);
	}
}