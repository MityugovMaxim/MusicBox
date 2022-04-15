using UnityEngine;

public class UIGrayscale : UIImage
{
	public float Grayscale
	{
		get => m_Grayscale;
		set
		{
			if (Mathf.Approximately(m_Grayscale, value))
				return;
			
			m_Grayscale = value;
			
			SetVerticesDirty();
		}
	}

	public float Brightness
	{
		get => m_Brightness;
		set
		{
			if (Mathf.Approximately(m_Brightness, value))
				return;
			
			m_Brightness = value;
			
			SetVerticesDirty();
		}
	}

	static readonly MaterialPool m_MaterialPool = new MaterialPool();

	[SerializeField, Range(0, 1)] float m_Grayscale;
	[SerializeField, Range(0, 1)] float m_Brightness;

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
		
		material = CreateMaterial("UI/Grayscale", keyword);
		
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
		return new Vector2(m_Grayscale, m_Brightness);
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
		return $"grayscale_{Type}_{Scheme}";
	}
}