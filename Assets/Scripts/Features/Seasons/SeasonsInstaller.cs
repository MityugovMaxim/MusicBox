using UnityEngine;

public class SeasonsInstaller : FeatureInstaller
{
	[SerializeField] UISeasonHeaderElement m_SeasonHeaderElement;
	[SerializeField] UISeasonLevelElement  m_SeasonLevelElement;

	public override void InstallBindings()
	{
		InstallSingleton<SeasonsCollection>();
		
		InstallSingleton<SeasonsDescriptors>();
		
		InstallSingleton<ProfileSeasons>();
		
		InstallSingleton<SeasonsManager>();
		
		InstallPool<UISeasonHeaderElement, UISeasonHeaderElement.Pool>(m_SeasonHeaderElement, 1);
		
		InstallPool<UISeasonLevelElement, UISeasonLevelElement.Pool>(m_SeasonLevelElement, 4);
	}
}
