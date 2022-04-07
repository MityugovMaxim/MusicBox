using System.Collections.Generic;
using TMPro;
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

	[SerializeField] GameObject    m_Header;
	[SerializeField] TMP_Text      m_Title;
	[SerializeField] RectTransform m_Container;

	[Inject] UISongItem.Pool m_ItemPool;

	readonly List<UISongItem> m_Items = new List<UISongItem>();

	public void Setup(string _Title, ICollection<string> _SongIDs)
	{
		Clear();
		
		m_Header.SetActive(!string.IsNullOrEmpty(_Title) && _SongIDs != null && _SongIDs.Count > 0);
		
		m_Title.text = _Title ?? string.Empty;
		
		if (_SongIDs == null || _SongIDs.Count == 0)
			return;
		
		foreach (string songID in _SongIDs)
		{
			if (string.IsNullOrEmpty(songID))
				continue;
			
			UISongItem item = m_ItemPool.Spawn(m_Container);
			
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