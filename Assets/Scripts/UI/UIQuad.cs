using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UIQuad : UIImage
{
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
			Sprite != null ? Sprite.rect.size : rect.size,
			m_Alignment,
			m_ScaleMode,
			m_BorderMode,
			m_Border.ToVector(),
			m_BorderScale,
			m_FillCenter
		);
		
		m_Generator.Fill(_VertexHelper, Sprite, color);
	}

	protected override Material GetMaterial() => defaultGraphicMaterial;

	protected override Vector4 GetUV2() => Vector4.zero;

	protected override Vector4 GetUV3() => Vector4.zero;

	protected override Vector3 GetNormal() => Vector3.zero;

	protected override Vector4 GetTangent() => Vector4.zero;
}
