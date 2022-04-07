using UnityEngine;
using UnityEngine.UI;

public class WebImage : RemoteImage<Image>
{
	protected override Sprite Sprite
	{
		get => Graphic.sprite;
		set => Graphic.sprite = value;
	}
}