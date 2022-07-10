using UnityEngine;

public class UITexture : UIMeshRenderer
{
	public Texture Texture
	{
		get => m_Texture;
		set
		{
			if (m_Texture == value)
				return;
			
			m_Texture = value;
			
			InvalidateProperties();
		}
	}

	protected override Texture MainTexture => m_Texture != null ? m_Texture : Texture2D.whiteTexture;

	protected override Rect UV { get; } = new Rect(0, 0, 1, 1);

	[SerializeField] Texture m_Texture;
}