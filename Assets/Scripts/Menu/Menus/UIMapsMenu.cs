using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.MapsMenu)]
public class UIMapsMenu : UIMenu
{
	const float LIST_SPACING = 15;

	[SerializeField] UILayout m_Content;
	[SerializeField] Button   m_BackButton;
	[SerializeField] Button   m_RestoreButton;
	[SerializeField] Button   m_ColorsButton;

	[Inject] SongsCollection    m_SongsCollection;
	[Inject] MenuProcessor     m_MenuProcessor;
	[Inject] UIMapElement.Pool m_Pool;

	protected override void Awake()
	{
		base.Awake();
		
		m_BackButton.onClick.AddListener(Back);
		m_ColorsButton.onClick.AddListener(Colors);
		m_RestoreButton.onClick.AddListener(Restore);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_BackButton.onClick.RemoveListener(Back);
		m_ColorsButton.onClick.RemoveListener(Colors);
		m_RestoreButton.onClick.RemoveListener(Restore);
	}

	void Back()
	{
		Hide();
	}

	void Restore()
	{
		Refresh();
	}

	async void Colors()
	{
		await m_MenuProcessor.Show(MenuType.ColorsMenu);
	}

	protected override void OnShowStarted()
	{
		Refresh();
	}

	void Refresh()
	{
		m_Content.Clear();
		
		IReadOnlyList<string> songIDs = m_SongsCollection.GetIDs();
		
		if (songIDs == null || songIDs.Count == 0)
			return;
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		foreach (string songID in songIDs)
		{
			if (string.IsNullOrEmpty(songID))
				continue;
			
			MapElementEntity item = new MapElementEntity(songID, m_Pool);
			
			m_Content.Add(item);
		}
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
		
		m_Content.Reposition();
	}
}
