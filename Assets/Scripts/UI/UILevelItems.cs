using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UILevelItems : UIGroup
{
	[SerializeField] RectTransform m_Container;

	[Inject] ProgressProcessor m_ProgressProcessor;
	[Inject] UIUnlockItem.Pool m_Pool;

	readonly List<UIUnlockItem> m_Items = new List<UIUnlockItem>();

	int m_Level;

	public void Setup(int _Level)
	{
		m_Level = _Level;
		
		foreach (UIUnlockItem item in m_Items)
			m_Pool.Despawn(item);
		m_Items.Clear();
		
		CreateCoins();
		
		CreateSongs();
	}

	void CreateCoins()
	{
		long coins = m_ProgressProcessor.GetCoins(m_Level);
		
		if (coins <= 0)
			return;
		
		UIUnlockItem item = m_Pool.Spawn(m_Container);
		
		if (item == null)
			return;
		
		item.Setup("Thumbnails/Coins/coins_1.jpg", coins);
		
		m_Items.Add(item);
	}

	void CreateSongs()
	{
		List<string> songIDs = m_ProgressProcessor.GetSongIDs(m_Level);
		
		if (songIDs == null || songIDs.Count == 0)
			return;
		
		foreach (string songID in songIDs)
		{
			UIUnlockItem item = m_Pool.Spawn(m_Container);
			
			if (item == null)
				continue;
			
			item.Setup($"Thumbnails/Songs/{songID}.jpg");
			
			m_Items.Add(item);
		}
	}

	public async Task PlayAsync()
	{
		foreach (UIUnlockItem item in m_Items)
		{
			await Task.WhenAny(
				item.PlayAsync(),
				Task.Delay(250)
			);
		}
	}
}