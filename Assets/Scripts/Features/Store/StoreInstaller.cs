public class StoreInstaller : FeatureInstaller
{
	public override void InstallBindings()
	{
		InstallSingleton<StoreCollection>();
		
		InstallSingleton<StoreProcessor>();
		
		InstallSingleton<StoreManager>();
	}
}
