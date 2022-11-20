public class StorageInstaller : FeatureInstaller
{
	public override void InstallBindings()
	{
		InstallSingleton<AudioClipProvider>();
		
		InstallSingleton<TextureProvider>();
		
		InstallSingleton<ASFProvider>();
		
		InstallSingleton<LocalizationProvider>();
	}
}
