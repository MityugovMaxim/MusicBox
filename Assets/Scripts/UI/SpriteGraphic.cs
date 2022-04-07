using UnityEngine;
using UnityEngine.UI;

public class SpriteGraphic : MaskableGraphic
{
	public Sprite Sprite
	{
		get => m_Sprite;
		set
		{
			if (m_Sprite == value)
				return;
			
			m_Sprite = value;
			
			SetMaterialDirty();
		}
	}

	public override Texture mainTexture => Sprite != null ? Sprite.texture : base.mainTexture;

	[SerializeField] Sprite m_Sprite;

	protected Vector2 GetSpriteUV(Vector2 _UV)
	{
		if (Sprite == null || Sprite.texture == null)
			return _UV;
		
		float width  = Sprite.texture.width;
		float height = Sprite.texture.height;
		Rect  rect   = Sprite.rect;
		return new Vector2(
			(rect.x + rect.width * _UV.x) / width,
			(rect.y + rect.height * _UV.y) / height
		);
	}
}