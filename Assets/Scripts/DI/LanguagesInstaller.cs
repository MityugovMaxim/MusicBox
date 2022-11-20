using UnityEngine;

public class LanguagesInstaller : FeatureInstaller
{
	[SerializeField] UILanguageItem m_LanguageItem;

	public override void InstallBindings()
	{
		InstallSingleton<LanguagesCollection>();
		
		InstallSingleton<LanguagesManager>();
		
		InstallSingleton<Localization>();
		
		InstallPool<UILanguageItem, UILanguageItem.Pool>(m_LanguageItem);
	}
}
