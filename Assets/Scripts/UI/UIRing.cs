using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UIRing : MaskableGraphic
{
	public Sprite Sprite
	{
		get => m_Sprite;
		set
		{
			if (m_Sprite == value)
				return;
			
			m_Sprite = value;
			
			SetMaterialDirty();
		}
	}

	public float Origin
	{
		get => m_Origin;
		set
		{
			if (Mathf.Approximately(m_Origin, value))
				return;
			
			m_Origin = value;
			
			SetVerticesDirty();
		}
	}

	public float Arc
	{
		get => m_Arc;
		set
		{
			if (Mathf.Approximately(m_Arc, value))
				return;
			
			m_Arc = value;
			
			SetVerticesDirty();
		}
	}

	public override Texture mainTexture => Sprite != null ? Sprite.texture : base.mainTexture;

	[SerializeField]              Sprite m_Sprite;
	[SerializeField]              int    m_Samples; 
	[SerializeField]              float  m_Radius;
	[SerializeField]              float  m_Width;
	[SerializeField, Range(0, 1)] float  m_Origin;
	[SerializeField, Range(0, 1)] float  m_Arc = 1;
	[SerializeField]              bool   m_Clockwise;

	readonly List<UIVertex> m_Vertices = new List<UIVertex>();
	readonly List<int>      m_Indices  = new List<int>();

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		_VertexHelper.Clear();
		
		Color32 color = this.color;
		
		if (m_Radius <= float.Epsilon || m_Width <= float.Epsilon || m_Arc <= float.Epsilon || color.a <= 0)
			return;
		
		m_Vertices.Clear();
		m_Indices.Clear();
		
		int samples = Mathf.Max(1, Mathf.FloorToInt(m_Samples * m_Arc));
		
		float      angle = 360.0f * m_Arc / samples;
		Quaternion step  = Quaternion.Euler(0, 0, m_Clockwise ? -angle : angle);
		
		Vector3 origin = Quaternion.Euler(0, 0, 360.0f * m_Origin) * Vector3.right;
		Vector3 outer  = origin * m_Radius;
		Vector3 inner  = origin * Mathf.Max(0, m_Radius - m_Width);
		Rect    uv     = new Rect(0, 0, 1, 1);
		if (Sprite != null)
		{
			uv = new Rect(
				Sprite.rect.x / Sprite.texture.width,
				Sprite.rect.y / Sprite.texture.height,
				Sprite.rect.width / Sprite.texture.width,
				Sprite.rect.height / Sprite.texture.height
			);
		}
		for (int i = 0; i <= samples; i++)
		{
			float phase = Mathf.Lerp(uv.yMax, uv.yMin, (float)i / samples);
			
			m_Vertices.Add(Create(outer, color, new Vector2(uv.xMin, phase)));
			m_Vertices.Add(Create(inner, color, new Vector2(uv.xMax, phase)));
			
			outer = step * outer;
			inner = step * inner;
		}
		
		if (m_Clockwise)
		{
			for (int i = 0; i < samples; i++)
			{
				m_Indices.Add(i * 2 + 3);
				m_Indices.Add(i * 2 + 1);
				m_Indices.Add(i * 2 + 0);
				m_Indices.Add(i * 2 + 3);
				m_Indices.Add(i * 2 + 0);
				m_Indices.Add(i * 2 + 2);
			}
		}
		else
		{
			for (int i = 0; i < samples; i++)
			{
				m_Indices.Add(i * 2 + 0);
				m_Indices.Add(i * 2 + 1);
				m_Indices.Add(i * 2 + 3);
				m_Indices.Add(i * 2 + 2);
				m_Indices.Add(i * 2 + 0);
				m_Indices.Add(i * 2 + 3);
			}
		}
		
		_VertexHelper.AddUIVertexStream(m_Vertices, m_Indices);
	}

	static UIVertex Create(Vector3 _Position, Color32 _Color, Vector2 _UV)
	{
		return new UIVertex()
		{
			position = _Position,
			color    = _Color,
			normal   = Vector3.forward,
			uv0      = _UV,
		};
	}
}
