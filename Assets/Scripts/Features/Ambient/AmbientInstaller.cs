using UnityEngine;

public class AmbientInstaller : FeatureInstaller
{
	[SerializeField] UIAmbientElement m_AmbientElement;

	public override void InstallBindings()
	{
		InstallSingleton<AmbientCollection>();
		
		InstallSingleton<AmbientManager>();
		
		InstallPool<UIAmbientElement, UIAmbientElement.Pool>(m_AmbientElement);
	}
}
