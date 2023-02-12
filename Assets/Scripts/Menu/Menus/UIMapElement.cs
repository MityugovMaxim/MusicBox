using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class UIMapElement : UIOverlayButton
{
	[Preserve]
	public class Pool : UIEntityPool<UIMapElement> { }

	[SerializeField] UISongImage m_Image;
	[SerializeField] UISongLabel m_Label;

	[Inject] MenuProcessor m_MenuProcessor;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Image.SongID = m_SongID;
		m_Label.SongID = m_SongID;
	}

	protected override void OnClick()
	{
		base.OnClick();
		
		Open();
	}

	async void Open()
	{
		UIMapMenu mapMenu = m_MenuProcessor.GetMenu<UIMapMenu>();
		
		if (mapMenu == null)
			return;
		
		mapMenu.Setup(m_SongID);
		
		UILoadingMenu loadingMenu = m_MenuProcessor.GetMenu<UILoadingMenu>();
		
		loadingMenu.Setup(m_SongID);
		
		await loadingMenu.ShowAsync();
		
		await mapMenu.Load();
		
		mapMenu.Show(true);
		
		await loadingMenu.HideAsync();
	}
}
