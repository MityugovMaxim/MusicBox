using UnityEngine;

public class NewsInstaller : FeatureInstaller
{
	[SerializeField] UINewsElement m_NewsElement;

	public override void InstallBindings()
	{
		InstallSingleton<NewsCollection>();
		
		InstallSingleton<NewsDescriptor>();
		
		InstallSingleton<NewsManager>();
		
		InstallPool<UINewsElement, UINewsElement.Pool>(m_NewsElement, 3);
	}
}
