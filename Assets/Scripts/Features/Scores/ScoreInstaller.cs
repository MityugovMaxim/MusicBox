public class ScoreInstaller : FeatureInstaller
{
	public override void InstallBindings()
	{
		InstallSingleton<ProfileScores>();
		
		InstallSingleton<ScoresManager>();
		
		InstallSingleton<ScoreController>();
	}
}
