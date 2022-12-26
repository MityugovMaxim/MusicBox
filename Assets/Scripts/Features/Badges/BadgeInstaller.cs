public class BadgeInstaller : FeatureInstaller
{
	public override void InstallBindings()
	{
		InstallSingleton<BadgeManager>();
	}
}