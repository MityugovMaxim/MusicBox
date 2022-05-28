using UnityEngine;
using Zenject;

[Menu(MenuType.TutorialMenu)]
public class UITutorialMenu : UIMenu
{
	public IASFSampler Sampler => m_Timeline;

	[SerializeField] UITimeline m_Timeline;

	[Inject] TutorialController m_TutorialController;

	public void Skip()
	{
		m_TutorialController.Skip();
	}
}