using UnityEngine;

public class SeasonsInstaller : FeatureInstaller
{
	[SerializeField] UISeasonHeaderElement m_SeasonHeaderElement;
	[SerializeField] UISeasonLevelElement  m_SeasonLevelElement;

	public override void InstallBindings()
	{
		InstallSingleton<SeasonsCollection>();
		
		InstallSingleton<SeasonsDescriptor>();
		
		InstallSingleton<ProfileSeasons>();
		
		InstallSingleton<SeasonsManager>();
		
		InstallPool<UISeasonHeaderElement, UISeasonHeaderElement.Pool>(m_SeasonHeaderElement);
		
		InstallPool<UISeasonLevelElement, UISeasonLevelElement.Pool>(m_SeasonLevelElement, 4);
	}
}
