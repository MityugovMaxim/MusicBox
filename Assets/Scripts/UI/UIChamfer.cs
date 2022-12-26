using UnityEngine;

[RequireComponent(typeof(CanvasRenderer))]
public class UIChamfer : UIImage
{
	static readonly MaterialPool m_MaterialPool = new MaterialPool();

	[SerializeField] float m_TopLeft;
	[SerializeField] float m_TopRight;
	[SerializeField] float m_BottomLeft;
	[SerializeField] float m_BottomRight;

	protected override Material GetMaterial()
	{
		string materialID = GetMaterialID();
		
		Material material = m_MaterialPool.Get(materialID);
		
		if (material != null)
			return material;
		
		material = CreateMaterial("Elements/Chamfer");
		
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

	protected override Vector4 GetUV2()
	{
		Rect rect = rectTransform.rect;
		
		return rect.ToVector();
	}

	protected override Vector4 GetUV3()
	{
		Rect rect = rectTransform.rect;
		
		float topLeft     = m_TopLeft;
		float topRight    = m_TopRight;
		float bottomLeft  = m_BottomLeft;
		float bottomRight = m_BottomRight;
		
		float top = m_TopLeft + m_TopRight;
		if (top > rect.width)
		{
			topLeft  = rect.width * (m_TopLeft / top);
			topRight = rect.width * (m_TopRight / top);
		}
		
		float bottom = m_BottomLeft + m_BottomRight;
		if (bottom > rect.width)
		{
			bottomLeft  = rect.width * (m_BottomLeft / bottom);
			bottomRight = rect.width * (m_BottomRight / bottom);
		}
		
		float left = m_TopLeft + m_BottomLeft;
		if (left > rect.height)
		{
			topLeft    = rect.height * (m_TopLeft / left);
			bottomLeft = rect.height * (m_BottomLeft / left);
		}
		
		float right = m_TopRight + m_BottomRight;
		if (right > rect.height)
		{
			topRight    =  rect.height * (m_TopRight / right);
			bottomRight = rect.height * (m_BottomRight / right);
		}
		
		return new Vector4(topLeft, topRight, bottomLeft, bottomRight);
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
		return $"chamfer_{Type}";
	}
}
