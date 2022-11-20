using UnityEngine;

public class OffersInstaller : FeatureInstaller
{
	[SerializeField] UIOfferItem m_OfferItem;
	[SerializeField] int         m_Capacity = 1;

	public override void InstallBindings()
	{
		InstallSingleton<OffersCollection>();
		
		InstallSingleton<OffersDescriptor>();
		
		InstallSingleton<OffersManager>();
		
		InstallPool<UIOfferItem, UIOfferItem.Pool>(m_OfferItem, m_Capacity);
	}
}
