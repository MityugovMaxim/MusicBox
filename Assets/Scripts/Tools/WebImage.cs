using UnityEngine;
using UnityEngine.UI;

public class WebImage : RemoteImage
{
	public override Sprite Sprite
	{
		get => m_Graphic.sprite;
		protected set => m_Graphic.sprite = value;
	}

	protected override MaskableGraphic Graphic => m_Graphic;

	[SerializeField] Image m_Graphic;
}