using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UIQuad : MaskableGraphic
{
	public override Texture mainTexture => m_Sprite != null ? m_Sprite.texture : base.mainTexture;

	public Sprite Sprite
	{
		get => m_Sprite;
		set
		{
			if (m_Sprite == value)
				return;
			
			m_Sprite = value;
			
			SetAllDirty();
		}
	}

	[SerializeField] Sprite     m_Sprite;
	[SerializeField] TextAnchor m_Alignment;
	[SerializeField] ScaleMode  m_ScaleMode;
	[SerializeField] BorderMode m_BorderMode;
	[SerializeField] RectOffset m_Border;
	[SerializeField] float      m_BorderScale;
	[SerializeField] bool       m_FillCenter = true;

	readonly QuadGenerator m_Generator = new QuadGenerator();

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		Rect rect = rectTransform.rect;
		
		m_Generator.Generate(
			rect,
			m_Sprite != null ? m_Sprite.rect.size : rect.size,
			m_Alignment,
			m_ScaleMode,
			m_BorderMode,
			m_Border.ToVector(),
			m_BorderScale,
			m_FillCenter
		);
		
		m_Generator.Fill(_VertexHelper, Sprite, color);
	}
}
