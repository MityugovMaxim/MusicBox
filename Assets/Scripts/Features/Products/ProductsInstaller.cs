using UnityEngine;

public class ProductsInstaller : FeatureInstaller
{
	[SerializeField] UIProductSeasonElement m_ProductSeasonElement;
	[SerializeField] UIProductCoinsElement  m_ProductCoinsElement;

	public override void InstallBindings()
	{
		InstallSingleton<ProductsCollection>();
		
		InstallSingleton<ProductsDescriptor>();
		
		InstallSingleton<ProfileProducts>();
		
		InstallSingleton<ProductsManager>();
		
		InstallPool<UIProductSeasonElement, UIProductSeasonElement.Pool>(m_ProductSeasonElement);
		
		InstallPool<UIProductCoinsElement, UIProductCoinsElement.Pool>(m_ProductCoinsElement);
	}
}
