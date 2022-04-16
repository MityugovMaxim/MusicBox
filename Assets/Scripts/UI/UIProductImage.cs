using UnityEngine;

public class UIProductImage : UIEntity
{
	[SerializeField] WebImage m_Image;

	string m_ProductID;

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		m_Image.Path = $"Thumbnails/Products/{m_ProductID}.jpg";
	}
}