using UnityEngine;

[ExecuteInEditMode]
public class UIOverlay : UIImage
{
	static readonly MaterialPool m_MaterialPool = new MaterialPool();

	[SerializeField] Rect m_Rect;

	protected override Material GetMaterial()
	{
		string materialID = GetMaterialID();
		
		Material material = m_MaterialPool.Get(materialID);
		
		if (material != null)
			return material;
		
		material = CreateMaterial("UI/Overlay");
		
		switch (Type)
		{
			case BlendType.Blend:
				SetBlend(material);
				break;
			
			case BlendType.Additive:
				SetAdditive(material);
				break;
			
			default:
				SetBlend(material);
				break;
		}
		
		m_MaterialPool.Register(materialID, material);
		
		return material;
	}

	protected override Vector2 GetUV2()
	{
		return Vector2.zero;
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
		return new Vector4(m_Rect.xMin, m_Rect.yMin, m_Rect.xMax, m_Rect.yMax);
	}

	string GetMaterialID()
	{
		return $"overlay_{Type}";
	}
}