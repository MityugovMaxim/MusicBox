using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIDailyElement : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UIDailyElement> { }

	[SerializeField] UIDailyItem[] m_Items;

	[Inject] DailyManager m_DailyManager;

	public void Setup()
	{
		List<string> dailyIDs = m_DailyManager.GetDailyIDs();
		
		int itemsCount = Mathf.Min(dailyIDs.Count, m_Items.Length);
		for (int i = 0; i < itemsCount; i++)
			m_Items[i].DailyID = dailyIDs[i];
	}
}
