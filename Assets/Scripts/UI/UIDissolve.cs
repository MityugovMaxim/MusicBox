using UnityEngine;

public class UIDissolve : UIImage
{
	static readonly int m_DissolveTexPropertyID = Shader.PropertyToID("_DissolveTex");

	static readonly MaterialPool m_MaterialPool = new MaterialPool();

	[SerializeField, Range(0, 1)] float     m_Threshold;
	[SerializeField, Range(0, 1)] float     m_Border;
	[SerializeField]              Texture2D m_Dissolve;

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
		
		material = CreateMaterial("UI/Dissolve", keyword);
		material.SetTexture(m_DissolveTexPropertyID, m_Dissolve);
		
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
		return new Vector2(m_Threshold, m_Border);
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
		return Vector4.zero;
	}

	string GetMaterialID()
	{
		int textureID = m_Dissolve != null ? m_Dissolve.GetInstanceID() : int.MinValue;
		
		return $"dissolve_{Type}_{Scheme}_{textureID}";
	}
}