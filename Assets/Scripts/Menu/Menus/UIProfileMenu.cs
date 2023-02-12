using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

[Menu(MenuType.ProfileMenu)]
public class UIProfileMenu : UIMenu
{
	const float ITEM_ASPECT  = 2.2f;
	const float GRID_SPACING = 30;
	const float LIST_SPACING = 15;

	[SerializeField] UILayout m_Content;
	[SerializeField] UIGroup  m_ContentGroup;
	[SerializeField] UIGroup  m_LoaderGroup;

	[Inject] ScoresManager   m_ScoresManager;
	[Inject] VouchersManager m_VouchersManager;

	[Inject] UIProfileDiscsElement.Pool m_DiscsPool;
	[Inject] UIProfileSongElement.Pool  m_SongsPool;
	[Inject] UIVoucherElement.Pool      m_VouchersPool;

	protected override async void OnShowStarted()
	{
		base.OnShowStarted();
		
		m_ContentGroup.Hide(true);
		m_LoaderGroup.Show(true);
		
		bool instant = await m_VouchersManager.Activate();
		
		m_ContentGroup.Show(instant);
		m_LoaderGroup.Show(instant);
		
		m_Content.Clear();
		
		CreateDiscs();
		
		CreateBestSong();
		
		CreateWorstSong();
		
		CreateVouchers();
		
		m_Content.Reposition();
	}

	void CreateDiscs()
	{
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		m_Content.Add(new ProfileDiscsElementEntity(m_DiscsPool));
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}

	void CreateBestSong()
	{
		string songID = m_ScoresManager.GetBestSongID();
		
		if (string.IsNullOrEmpty(songID))
			return;
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		m_Content.Add(new ProfileSongElementEntity(songID, ProfileSongMode.Best, m_SongsPool));
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}

	void CreateWorstSong()
	{
		string songID = m_ScoresManager.GetWorstSongID();
		
		if (string.IsNullOrEmpty(songID))
			return;
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		m_Content.Add(new ProfileSongElementEntity(songID, ProfileSongMode.Worst, m_SongsPool));
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}

	void CreateVouchers()
	{
		List<string> voucherIDs = m_VouchersManager.GetVoucherIDs();
		
		if (voucherIDs == null || voucherIDs.Count == 0)
			return;
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		int count = voucherIDs.Count % 3;
		
		foreach (string voucherID in voucherIDs.Take(count))
			m_Content.Add(new VoucherElementEntity(voucherID, m_VouchersPool));
		
		VerticalStackLayout.End(m_Content);
		
		if (count > 0)
			m_Content.Space(LIST_SPACING);
		
		VerticalGridLayout.Start(m_Content, 3, ITEM_ASPECT, GRID_SPACING, GRID_SPACING);
		
		foreach (string voucherID in voucherIDs.Skip(count))
			m_Content.Add(new VoucherElementEntity(voucherID, m_VouchersPool));
		
		VerticalGridLayout.End(m_Content);
		
		if (voucherIDs.Count > count)
			m_Content.Space(LIST_SPACING);
	}
}
