using UnityEngine;
using UnityEngine.UI;

public class WebGraphic : RemoteImage
{
	public override Sprite Sprite
	{
		get => m_Graphic.Sprite;
		protected set => m_Graphic.Sprite = value;
	}

	protected override MaskableGraphic Graphic => m_Graphic;

	[SerializeField] UIImage m_Graphic;
}