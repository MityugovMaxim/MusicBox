using UnityEngine;
using Zenject;

public class UIMainMenuProfilePage : UIMainMenuPage
{
	const float LIST_SPACING = 15;

	public override MainMenuPageType Type => MainMenuPageType.Profile;

	[SerializeField] UILayout m_Content;
	[SerializeField] UIGroup  m_ContentGroup;
	[SerializeField] UIGroup  m_LoaderGroup;

	[Inject] UIAmbientElement.Pool m_AmbientPool;

	protected override void OnShowStarted()
	{
		m_ContentGroup.Hide(true);
		m_LoaderGroup.Show(true);
		
		int frame = Time.frameCount;
		
		// TODO: Preload
		
		bool instant = frame == Time.frameCount;
		
		m_ContentGroup.Show(instant);
		m_LoaderGroup.Hide(instant);
		
		Refresh();
	}

	void Refresh()
	{
		m_Content.Clear();
		
		CreateAmbient();
		
		m_Content.Reposition();
	}

	void CreateAmbient()
	{
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		m_Content.Add(new AmbientElementEntity(m_AmbientPool));
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}
}
