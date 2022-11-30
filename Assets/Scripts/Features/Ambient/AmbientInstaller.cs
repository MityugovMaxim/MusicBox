using UnityEngine;

public class AmbientInstaller : FeatureInstaller
{
	[SerializeField] AmbientSource    m_AmbientSource;
	[SerializeField] UIAmbientElement m_AmbientElement;

	public override void InstallBindings()
	{
		InstallSingleton<AmbientCollection>();
		
		InstallSingleton<AmbientManager>();
		
		InstallPool<AmbientSource, AmbientSource.Pool>(m_AmbientSource, 1);
		
		InstallPool<UIAmbientElement, UIAmbientElement.Pool>(m_AmbientElement, 1);
	}
}
