using Zenject;

public abstract class UIProductEntity : UIEntity
{
	public string ProductID
	{
		get => m_ProductID;
		set
		{
			if (m_ProductID == value)
				return;
			
			if (!string.IsNullOrEmpty(m_ProductID))
				Unsubscribe();
			
			m_ProductID = value;
			
			ProcessData();
			
			if (!string.IsNullOrEmpty(m_ProductID))
				Subscribe();
		}
	}

	protected ProductsManager ProductsManager => m_ProductsManager;

	[Inject] ProductsManager m_ProductsManager;

	string m_ProductID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		ProductID = null;
	}

	protected abstract void Subscribe();

	protected abstract void Unsubscribe();

	protected abstract void ProcessData();
}
