using UnityEngine;

public class OffersInstaller : FeatureInstaller
{
	[SerializeField] UIOfferElement m_OfferElement;
	[SerializeField] int         m_Capacity = 1;

	public override void InstallBindings()
	{
		InstallSingleton<OffersCollection>();
		
		InstallSingleton<OffersDescriptor>();
		
		InstallSingleton<OffersManager>();
		
		InstallPool<UIOfferElement, UIOfferElement.Pool>(m_OfferElement, m_Capacity);
	}
}
