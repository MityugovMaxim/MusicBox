using UnityEngine;
using Zenject;

public class UIProductImage : UIEntity
{
	public string ProductID
	{
		get => m_ProductID;
		set
		{
			if (m_ProductID == value)
				return;
			
			m_ProductsManager.Collection.Unsubscribe(DataEventType.Change, m_ProductID, ProcessImage);
			
			m_ProductID = value;
			
			m_ProductsManager.Collection.Subscribe(DataEventType.Change, m_ProductID, ProcessImage);
			
			ProcessImage();
		}
	}

	[SerializeField] WebImage m_Image;

	[Inject] ProductsManager m_ProductsManager;

	string m_ProductID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		ProductID = null;
	}

	void ProcessImage()
	{
		m_Image.Path = m_ProductsManager.GetImage(ProductID);
	}
}
