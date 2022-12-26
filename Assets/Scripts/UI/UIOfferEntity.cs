using Zenject;

public abstract class UIOfferEntity : UIEntity
{
	public string OfferID
	{
		get => m_OfferID;
		set
		{
			if (m_OfferID == value)
				return;
			
			if (!string.IsNullOrEmpty(m_OfferID))
				Unsubscribe();
			
			m_OfferID = value;
			
			ProcessData();
			
			if (!string.IsNullOrEmpty(m_OfferID))
				Subscribe();
		}
	}

	protected OffersManager OffersManager => m_OffersManager;

	[Inject] OffersManager m_OffersManager;

	string m_OfferID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		OfferID = null;
	}

	protected abstract void Subscribe();

	protected abstract void Unsubscribe();

	protected abstract void ProcessData();
}