using UnityEngine;

public class UICircle : UIImage
{
	static readonly MaterialPool m_MaterialPool = new MaterialPool();

	[SerializeField, Range(0, 1)] float m_Size;
	[SerializeField, Range(0, 1)] float m_Fade;

	protected override Material GetMaterial()
	{
		string materialID = GetMaterialID();
		
		Material material = m_MaterialPool.Get(materialID);
		
		if (material != null)
			return material;
		
		string keyword;
		switch (Scheme)
		{
			case BlendScheme.Background:
				keyword = "BACKGROUND_SCHEME";
				break;
			case BlendScheme.Foreground:
				keyword = "FOREGROUND_SCHEME";
				break;
			default:
				keyword = null;
				break;
		}
		
		material = CreateMaterial("UI/Circle", keyword);
		
		switch (Type)
		{
			case BlendType.Blend:
				SetBlend(material);
				break;
			
			case BlendType.Additive:
				SetAdditive(material);
				break;
		}
		
		m_MaterialPool.Register(materialID, material);
		
		return material;
	}

	protected override Vector2 GetUV2()
	{
		return new Vector2(m_Size, m_Fade);
	}

	protected override Vector2 GetUV3()
	{
		return Vector2.zero;
	}

	protected override Vector3 GetNormal()
	{
		return Vector3.zero;
	}

	protected override Vector4 GetTangent()
	{
		if (Sprite == null)
			return new Vector4(0, 0, 1, 1);
		
		return new Vector4(
			Sprite.rect.x / Sprite.texture.width,
			Sprite.rect.y / Sprite.texture.height,
			Sprite.rect.width / Sprite.texture.width,
			Sprite.rect.height / Sprite.texture.height
		);
	}

	string GetMaterialID()
	{
		return $"circle_{Type}_{Scheme}";
	}
}