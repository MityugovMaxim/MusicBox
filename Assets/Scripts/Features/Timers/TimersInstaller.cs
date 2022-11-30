public class TimersInstaller : FeatureInstaller
{
	public override void InstallBindings()
	{
		InstallSingleton<ProfileTimers>();
		
		InstallSingleton<TimersManager>();
	}
}
