using UnityEngine;

public class ChestsInstaller : FeatureInstaller
{
	[SerializeField] UIChestElement m_ChestElement;

	public override void InstallBindings()
	{
		InstallSingleton<ChestsCollection>();
		
		InstallSingleton<ProfileChests>();
		
		InstallSingleton<ChestsManager>();
		
		InstallPool<UIChestElement, UIChestElement.Pool>(m_ChestElement, 6);
	}
}
