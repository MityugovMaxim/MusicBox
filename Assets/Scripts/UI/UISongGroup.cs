using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UISongGroup : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UISongGroup>
	{
		protected override void OnDespawned(UISongGroup _Item)
		{
			base.OnDespawned(_Item);
			
			_Item.Clear();
		}
	}

	[SerializeField] RectTransform m_Container;

	[Inject] UISongItem.Pool m_ItemPool;

	readonly List<UISongItem> m_Items = new List<UISongItem>();

	public void Setup(ICollection<string> _SongIDs)
	{
		Clear();
		
		Fill(_SongIDs);
	}

	void Fill(ICollection<string> _SongIDs)
	{
		if (_SongIDs == null || _SongIDs.Count == 0)
			return;
		
		foreach (string songID in _SongIDs)
		{
			if (string.IsNullOrEmpty(songID))
				continue;
			
			UISongItem item = m_ItemPool.Spawn(m_Container);
			
			if (item == null)
				continue;
			
			item.Setup(songID);
			
			m_Items.Add(item);
		}
	}

	void Clear()
	{
		foreach (UISongItem item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
	}
}