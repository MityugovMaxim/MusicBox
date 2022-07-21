using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UILevelItems : UIGroup
{
	[SerializeField] RectTransform m_Container;
	[SerializeField] SongPreview   m_Preview;

	[Inject] ProgressProcessor      m_ProgressProcessor;
	[Inject] UIUnlockCoinsItem.Pool m_CoinsPool;
	[Inject] UIUnlockSongItem.Pool  m_SongsPool;

	readonly List<UIUnlockItem> m_Items = new List<UIUnlockItem>();

	int m_Level;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_Preview.Stop();
	}

	public void Setup(int _Level)
	{
		m_Level = _Level;
		
		foreach (UIUnlockCoinsItem item in m_Items.OfType<UIUnlockCoinsItem>())
			m_CoinsPool.Despawn(item);
		foreach (UIUnlockSongItem item in m_Items.OfType<UIUnlockSongItem>())
			m_SongsPool.Despawn(item);
		m_Items.Clear();
		
		CreateCoins();
		
		CreateSongs();
	}

	protected override void OnHideStarted()
	{
		base.OnHideStarted();
		
		m_Preview.Stop();
	}

	void CreateCoins()
	{
		long coins = m_ProgressProcessor.GetCoins(m_Level);
		
		if (coins <= 0)
			return;
		
		UIUnlockCoinsItem item = m_CoinsPool.Spawn(m_Container);
		
		if (item == null)
			return;
		
		item.Setup(coins);
		
		m_Items.Add(item);
	}

	void CreateSongs()
	{
		List<string> songIDs = m_ProgressProcessor.GetSongIDs(m_Level);
		
		if (songIDs == null || songIDs.Count == 0)
			return;
		
		foreach (string songID in songIDs)
		{
			UIUnlockSongItem item = m_SongsPool.Spawn(m_Container);
			
			if (item == null)
				continue;
			
			item.Setup(songID, m_Preview);
			
			m_Items.Add(item);
		}
	}

	public async Task PlayAsync()
	{
		m_Preview.Stop();
		
		foreach (UIUnlockItem item in m_Items)
		{
			await Task.WhenAny(
				item.PlayAsync(),
				Task.Delay(250)
			);
		}
	}
}