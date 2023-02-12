using UnityEngine;

public class UIWave : UIImage
{
	static readonly MaterialPool m_MaterialPool = new MaterialPool();

	[SerializeField] float   m_Frequency;
	[SerializeField] float   m_Speed;
	[SerializeField] Vector2 m_Amplitude;

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
		
		item = CreateMaterial("UI/Wave", keyword);
		
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
		return Vector4.zero;
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
		return new Vector4(
			m_Frequency,
			m_Speed,
			m_Amplitude.x,
			m_Amplitude.y
		);
	}

	string GetMaterialID()
	{
		return $"wave_{Type}_{Scheme}";
	}
}
