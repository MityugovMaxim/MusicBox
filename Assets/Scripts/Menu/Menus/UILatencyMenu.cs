using UnityEngine;
using Zenject;

[Menu(MenuType.LatencyMenu)]
public class UILatencyMenu : UISlideMenu
{
	[SerializeField] UILatencyIndicator m_LatencyIndicator;

	[Inject] AudioManager     m_AudioManager;
	[Inject] AmbientManager m_AmbientManager;

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		m_AmbientManager.Pause();
		
		m_LatencyIndicator.Process();
		
		m_AudioManager.OnSourceChange += OnSourceChange;
	}

	protected override void OnHideStarted()
	{
		base.OnHideStarted();
		
		m_LatencyIndicator.Complete();
		
		m_AudioManager.OnSourceChange -= OnSourceChange;
	}

	protected override void OnHideFinished()
	{
		base.OnHideFinished();
		
		m_AmbientManager.Play();
	}

	protected override bool OnEscape()
	{
		Hide();
		
		return true;
	}

	void OnSourceChange()
	{
		m_LatencyIndicator.Process();
	}
}
