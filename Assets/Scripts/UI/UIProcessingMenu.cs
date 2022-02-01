using UnityEngine;

[Menu(MenuType.ProcessingMenu)]
public class UIProcessingMenu : UIMenu
{
	[SerializeField] UILoader m_Loader;

	protected override void OnShowStarted()
	{
		if (m_Loader != null)
			m_Loader.Restore();
	}
}
