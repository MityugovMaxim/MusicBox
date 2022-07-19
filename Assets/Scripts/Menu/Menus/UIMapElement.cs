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
		
		m_Image.Setup(m_SongID);
		m_Label.Setup(m_SongID);
	}

	protected override void OnClick()
	{
		base.OnClick();
		
		Open();
	}

	void Open()
	{
		UIMapEditMenu mapEditMenu = m_MenuProcessor.GetMenu<UIMapEditMenu>();
		
		if (mapEditMenu == null)
			return;
		
		mapEditMenu.Setup(m_SongID);
		
		mapEditMenu.Show();
	}
}