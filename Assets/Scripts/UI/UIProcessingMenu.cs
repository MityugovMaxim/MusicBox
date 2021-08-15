using UnityEngine;

public class UIProcessingMenu : UIMenu
{
	[SerializeField] UILoader m_Loader;

	protected override void OnShowStarted()
	{
		if (m_Loader != null)
			m_Loader.Restore();
	}

	protected override void OnShowFinished()
	{
		if (m_Loader != null)
			m_Loader.Play();
	}
}
