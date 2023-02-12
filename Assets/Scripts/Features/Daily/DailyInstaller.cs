using UnityEngine;

public class DailyInstaller : FeatureInstaller
{
	[SerializeField] UIDailyElement m_DailyElement;
	[SerializeField] int            m_Capacity = 1;

	public override void InstallBindings()
	{
		InstallSingleton<DailyCollection>();
		
		InstallSingleton<ProfileDaily>();
		
		InstallSingleton<DailyManager>();
		
		InstallPool<UIDailyElement, UIDailyElement.Pool>(m_DailyElement, m_Capacity);
	}
}
