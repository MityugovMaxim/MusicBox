using UnityEngine;

public class WebGraphic : RemoteImage<UIImage>
{
	public override Sprite Sprite
	{
		get => Graphic.Sprite;
		set => Graphic.Sprite = value;
	}
}