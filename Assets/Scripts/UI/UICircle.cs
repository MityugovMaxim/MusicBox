using UnityEngine;

public class UICircle : UIImage
{
	public float Size
	{
		get => m_Size;
		set
		{
			if (Mathf.Approximately(m_Size, value))
				return;
			
			m_Size = value;
			
			SetVerticesDirty();
		}
	}

	static readonly MaterialPool m_MaterialPool = new MaterialPool();

	[SerializeField, Range(0, 1)] float m_Size;
	[SerializeField, Range(0, 1)] float m_Fade;

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
		
		item = CreateMaterial("UI/Circle", keyword);
		
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
		return new Vector2(m_Size, m_Fade);
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
		return $"circle_{Type}_{Scheme}";
	}
}
