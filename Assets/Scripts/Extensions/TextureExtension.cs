using UnityEngine;

public static class TextureExtension
{
	public static Texture2D CreateBlur(this Texture2D _Texture, float _Scale = 1, int _Iterations = 3)
	{
		return BlurUtility.Blur(_Texture, _Scale, _Iterations);
	}

	public static Sprite CreateSprite(this Texture2D _Texture)
	{
		Sprite sprite = Sprite.Create(
			_Texture,
			new Rect(0, 0, _Texture.width, _Texture.height),
			new Vector2(_Texture.width, _Texture.height) * 0.5f,
			1,
			0,
			SpriteMeshType.FullRect,
			Vector4.zero
		);
		
		sprite.name = _Texture.name;
		
		return sprite;
	}
}