using UnityEngine;

public static class TextureExtension
{
	public static Texture2D CreateBlur(this Texture2D _Texture, float _Scale = 1, int _Iterations = 3)
	{
		return BlurUtility.Blur(_Texture, _Scale, _Iterations);
	}

	public static Texture2D SetSize(this Texture2D _Texture, int _Size)
	{
		return _Texture.SetSize(_Size, _Size);
	}

	public static Texture2D SetSize(this Texture2D _Texture, int _Width, int _Height)
	{
		if (_Texture.width == _Width && _Texture.height == _Height)
			return _Texture;
		
		Texture2D texture = new Texture2D(_Width, _Height, _Texture.format, false, false);
		
		RenderTexture buffer = RenderTexture.GetTemporary(_Width, _Height);
		
		Graphics.Blit(_Texture, buffer);
		
		RenderTexture active = RenderTexture.active;
		
		RenderTexture.active = buffer;
		
		texture.ReadPixels(new Rect(0, 0, _Width, _Height), 0, 0);
		
		RenderTexture.active = active;
		
		texture.Apply(false, true);
		
		RenderTexture.ReleaseTemporary(buffer);
		
		return texture;
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