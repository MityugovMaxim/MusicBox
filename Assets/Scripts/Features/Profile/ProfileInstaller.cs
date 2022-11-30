using UnityEngine;

public class ProfileInstaller : FeatureInstaller
{
	[SerializeField] UIProfileElement      m_ProfileElement;
	[SerializeField] UIProfileDiscsElement m_ProfileDiscsElement;

	public override void InstallBindings()
	{
		InstallSingleton<ProfileCoinsParameter>();
		
		InstallSingleton<ProfileLevelParameter>();
		
		InstallSingleton<ProfileDiscsParameter>();
		
		InstallSingleton<ProfileFrameParameter>();
		
		InstallPool<UIProfileElement, UIProfileElement.Pool>(m_ProfileElement);
		
		InstallPool<UIProfileDiscsElement, UIProfileDiscsElement.Pool>(m_ProfileDiscsElement);
	}
}
