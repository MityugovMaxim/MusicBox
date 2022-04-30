using UnityEngine;
using UnityEngine.UI;

public class WebImage : RemoteImage<Image>
{
	public override Sprite Sprite
	{
		get => Graphic.sprite;
		set => Graphic.sprite = value;
	}
}