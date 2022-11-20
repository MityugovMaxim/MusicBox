public class TimersInstaller : FeatureInstaller
{
	public override void InstallBindings()
	{
		InstallSingleton<TimersCollection>();
		
		InstallSingleton<TimersManager>();
	}
}
