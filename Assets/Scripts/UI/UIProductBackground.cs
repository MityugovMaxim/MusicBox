using Zenject;

public class UIProductBackground : UIBackground
{
	public string ProductID
	{
		get => m_ProductID;
		set
		{
			if (m_ProductID == value)
				return;
			
			m_ProductsManager.Collection.Unsubscribe(DataEventType.Change, m_ProductID, ProcessBackground);
			
			m_ProductID = value;
			
			m_ProductsManager.Collection.Subscribe(DataEventType.Change, m_ProductID, ProcessBackground);
			
			ProcessBackground();
		}
	}

	[Inject] ProductsManager m_ProductsManager;

	string m_ProductID;

	void ProcessBackground()
	{
		string image = m_ProductsManager.GetImage(ProductID);
		
		Show(image);
	}
}
