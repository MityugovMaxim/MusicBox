using UnityEngine;
using Zenject;

public class UIProductImage : UIEntity
{
	[SerializeField] WebImage m_Image;

	[Inject] ProductsProcessor m_ProductsProcessor;

	string m_ProductID;

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		m_Image.Path = m_ProductsProcessor.GetImage(m_ProductID);
	}
}