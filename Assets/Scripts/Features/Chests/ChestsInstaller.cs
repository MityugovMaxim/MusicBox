public class ChestsInstaller : FeatureInstaller
{
	public override void InstallBindings()
	{
		InstallSingleton<ChestsCollection>();
		
		InstallSingleton<ProfileBronzeChests>();
		InstallSingleton<ProfileSilverChests>();
		InstallSingleton<ProfileGoldChests>();
		InstallSingleton<ProfilePlatinumChests>();
		
		InstallSingleton<ChestSlots>();
		
		InstallSingleton<ChestsManager>();
	}
}
