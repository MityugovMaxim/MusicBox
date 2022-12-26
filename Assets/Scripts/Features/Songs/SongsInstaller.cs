using UnityEngine;

public class SongsInstaller : FeatureInstaller
{
	[SerializeField] UISongItem    m_SongItem;
	[SerializeField] UISongElement m_SongElement;

	public override void InstallBindings()
	{
		InstallSingleton<SongsCollection>();
		
		InstallSingleton<ProfileSongs>();
		
		InstallSingleton<SongsManager>();
		
		InstallPool<UISongItem, UISongItem.Pool>(m_SongItem, 4);
		
		InstallPool<UISongElement, UISongElement.Pool>(m_SongElement, 6);
	}
}