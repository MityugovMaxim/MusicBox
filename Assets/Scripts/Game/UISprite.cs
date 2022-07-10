using UnityEngine;

public class UISprite : UIMeshRenderer
{
	public Sprite Sprite
	{
		get => m_Sprite;
		set
		{
			if (m_Sprite == value)
				return;
			
			m_Sprite = value;
			
			InvalidateMesh();
			
			InvalidateProperties();
		}
	}

	protected override Texture MainTexture => m_Sprite != null ? m_Sprite.texture : Texture2D.whiteTexture;
	protected override Rect    UV          => MeshUtility.GetUV(m_Sprite);

	[SerializeField] Sprite m_Sprite;
}