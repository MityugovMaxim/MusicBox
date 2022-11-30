using UnityEngine;

public class UIProductImage : UIProductEntity
{
	[SerializeField] WebImage m_Image;

	protected override void Subscribe()
	{
		ProductsManager.Collection.Subscribe(DataEventType.Change, ProcessData);
	}

	protected override void Unsubscribe()
	{
		ProductsManager.Collection.Unsubscribe(DataEventType.Change, ProcessData);
	}

	protected override void ProcessData()
	{
		m_Image.Path = ProductsManager.GetImage(ProductID);
	}
}
