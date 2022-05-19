using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UISongList : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UISongList>
	{
		protected override void OnDespawned(UISongList _Item)
		{
			base.OnDespawned(_Item);
			
			_Item.Clear();
		}
	}

	[SerializeField] RectTransform m_Container;

	[Inject] UISongElement.Pool m_ItemPool;

	readonly List<UISongElement> m_Items = new List<UISongElement>();

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
			
			UISongElement item = m_ItemPool.Spawn(m_Container);
			
			if (item == null)
				continue;
			
			item.Setup(songID);
			
			m_Items.Add(item);
		}
	}

	void Clear()
	{
		foreach (UISongElement item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
	}
}