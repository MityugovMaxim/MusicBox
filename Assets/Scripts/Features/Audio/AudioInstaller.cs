using UnityEngine;

public class AudioInstaller : FeatureInstaller
{
	[SerializeField] AudioEntity m_AudioEntity;

	public override void InstallBindings()
	{
		InstallSingleton<AudioProcessor>();
		
		InstallFactory<AudioChannelType, AudioChannelSettings, AudioChannel, AudioChannel.Factory>();
		
		InstallPool<AudioEntity, AudioEntity.Pool>(m_AudioEntity, 3);
	}
}
