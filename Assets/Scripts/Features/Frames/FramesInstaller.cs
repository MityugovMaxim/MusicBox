using UnityEngine;

public class FramesInstaller : FeatureInstaller
{
	[SerializeField] UIFrameElement m_FrameElement;

	public override void InstallBindings()
	{
		InstallSingleton<FramesCollection>();
		
		InstallSingleton<ProfileFrames>();
		
		InstallSingleton<FramesManager>();
		
		InstallPool<UIFrameElement, UIFrameElement.Pool>(m_FrameElement, 4);
	}
}
