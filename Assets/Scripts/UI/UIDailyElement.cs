using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIDailyElement : UIOverlayButton
{
	[Preserve]
	public class Pool : UIEntityPool<UIDailyElement> { }

	[SerializeField] UIGroup       m_LoaderGroup;
	[SerializeField] UIDailyItem[] m_Items;

	[Inject] DailyManager m_DailyManager;

	public void Setup()
	{
		List<string> dailyIDs = m_DailyManager.GetDailyIDs();
		
		int length = Mathf.Min(dailyIDs.Count, m_Items.Length);
		for (int i = 0; i < length; i++)
			m_Items[i].DailyID = dailyIDs[i];
	}

	protected override async void OnClick()
	{
		base.OnClick();
		
		m_LoaderGroup.Show();
		
		await m_DailyManager.Collect();
		
		m_LoaderGroup.Hide();
	}
}
