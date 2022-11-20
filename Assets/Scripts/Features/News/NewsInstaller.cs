using UnityEngine;

public class NewsInstaller : FeatureInstaller
{
	[SerializeField] UINewsItem m_NewsItem;
	[SerializeField] int        m_Capacity = 1;

	public override void InstallBindings()
	{
		InstallSingleton<NewsCollection>();
		
		InstallSingleton<NewsDescriptor>();
		
		InstallSingleton<NewsManager>();
		
		InstallPool<UINewsItem, UINewsItem.Pool>(m_NewsItem, m_Capacity);
	}
}
