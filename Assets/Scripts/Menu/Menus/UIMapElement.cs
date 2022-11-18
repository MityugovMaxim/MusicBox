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
		UISongEditMenu songEditMenu = m_MenuProcessor.GetMenu<UISongEditMenu>();
		
		if (songEditMenu == null)
			return;
		
		songEditMenu.Setup(m_SongID);
		
		UILoadingMenu loadingMenu = m_MenuProcessor.GetMenu<UILoadingMenu>();
		
		loadingMenu.Setup(m_SongID);
		
		await loadingMenu.ShowAsync();
		
		await songEditMenu.Load();
		
		songEditMenu.Show(true);
		
		await loadingMenu.HideAsync();
	}
}
