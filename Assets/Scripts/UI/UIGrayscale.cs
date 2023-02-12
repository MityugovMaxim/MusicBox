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
		
		Material item = m_MaterialPool.Get(materialID);
		
		if (item != null)
			return item;
		
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
		
		item = CreateMaterial("UI/Grayscale", keyword);
		
		switch (Type)
		{
			case BlendType.Blend:
				SetBlend(item);
				break;
			
			case BlendType.Additive:
				SetAdditive(item);
				break;
		}
		
		m_MaterialPool.Register(materialID, item);
		
		return item;
	}

	protected override Vector4 GetUV2()
	{
		return new Vector4(m_Grayscale, m_Brightness);
	}

	protected override Vector4 GetUV3()
	{
		return Vector4.zero;
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
