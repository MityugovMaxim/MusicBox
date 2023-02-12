public class RevivesInstaller : FeatureInstaller
{
	public override void InstallBindings()
	{
		InstallSingleton<RevivesCollection>();
		
		InstallSingleton<RevivesManager>();
	}
}