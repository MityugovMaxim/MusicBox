public class DifficultyInstaller : FeatureInstaller
{
	public override void InstallBindings()
	{
		InstallSingleton<DifficultyCollection>();
		
		InstallSingleton<DifficultyManager>();
	}
}