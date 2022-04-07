using UnityEngine;

public class WebGraphic : RemoteImage<UIImage>
{
	protected override Sprite Sprite
	{
		get => Graphic.Sprite;
		set => Graphic.Sprite = value;
	}
}