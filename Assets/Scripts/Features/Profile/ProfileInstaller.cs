using UnityEngine;

public class ProfileInstaller : FeatureInstaller
{
	[SerializeField] UIProfileElement      m_ProfileElement;
	[SerializeField] UIProfileDiscsElement m_ProfileDiscsElement;
	[SerializeField] UIProfileSongElement  m_ProfileSongElement;

	public override void InstallBindings()
	{
		InstallSingleton<ProfileCoinsParameter>();
		
		InstallSingleton<ProfileLevelParameter>();
		
		InstallSingleton<ProfileDiscsParameter>();
		
		InstallSingleton<ProfileFrameParameter>();
		
		InstallPool<UIProfileElement, UIProfileElement.Pool>(m_ProfileElement);
		
		InstallPool<UIProfileDiscsElement, UIProfileDiscsElement.Pool>(m_ProfileDiscsElement);
		
		InstallPool<UIProfileSongElement, UIProfileSongElement.Pool>(m_ProfileSongElement, 2);
	}
}
