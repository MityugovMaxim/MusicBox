using UnityEngine;

public class SoundInstaller : FeatureInstaller
{
	[SerializeField] SoundSource m_SoundSource;

	public override void InstallBindings()
	{
		InstallSingleton<SoundProcessor>();
		
		InstallPool<SoundSource, SoundSource.Pool>(m_SoundSource);
	}
}
