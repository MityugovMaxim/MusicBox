using UnityEngine;

public static class BlurUtility
{
	static Material Material
	{
		get
		{
			if (m_Material == null)
				m_Material = new Material(Shader.Find("UI/Blur"));
			return m_Material;
		}
	}

	static Material m_Material;

	public static Sprite Blur(
		Sprite _Sprite,
		float  _Scale      = 1,
		int    _Iterations = 3 
	)
	{
		int width  = (int)_Sprite.rect.width;
		int height = (int)_Sprite.rect.height;
		
		RenderTexture source = RenderTexture.GetTemporary(width >> 1, height >> 1, 0);
		RenderTexture target = RenderTexture.GetTemporary(width >> 2, height >> 2, 0);
		
		Graphics.Blit(_Sprite.texture, source);
		
		for (int i = 0; i < _Iterations; i++)
		{
			Graphics.Blit(source, target, Material);
			Graphics.Blit(target, source, Material);
		}
		
		Graphics.Blit(source, target, Material);
		Graphics.Blit(
			target,
			source,
			Vector2.one * _Scale,
			Vector2.one * (1 - _Scale) * 0.5f
		);
		
		Texture2D texture = new Texture2D(
			source.width,
			source.height,
			TextureFormat.RGB24,
			false,
			false
		);
		
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = source;
		texture.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
		texture.Apply();
		RenderTexture.active = active;
		
		RenderTexture.ReleaseTemporary(source);
		RenderTexture.ReleaseTemporary(target);
		
		return Sprite.Create(
			texture,
			new Rect(2, 2, texture.width - 4, texture.height - 4),
			new Vector2(0.5f, 0.5f),
			1
		);
	}
}