using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIProductGroup : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UIProductGroup>
	{
		protected override void OnDespawned(UIProductGroup _Item)
		{
			base.OnDespawned(_Item);
			
			_Item.Clear();
		}
	}

	[SerializeField] GameObject    m_Header;
	[SerializeField] TMP_Text      m_Title;
	[SerializeField] RectTransform m_Container;

	[Inject] UIProductItem.Pool m_ItemPool;

	readonly List<UIProductItem> m_Items = new List<UIProductItem>();

	public void Setup(string _Title, ICollection<string> _ProductIDs)
	{
		Clear();
		
		m_Header.SetActive(!string.IsNullOrEmpty(_Title) && _ProductIDs != null && _ProductIDs.Count > 0);
		
		m_Title.text = _Title ?? string.Empty;
		
		if (_ProductIDs == null || _ProductIDs.Count == 0)
			return;
		
		foreach (string productID in _ProductIDs)
		{
			if (string.IsNullOrEmpty(productID))
				continue;
			
			UIProductItem item = m_ItemPool.Spawn(m_Container);
			
			item.Setup(productID);
			
			m_Items.Add(item);
		}
	}

	void Clear()
	{
		foreach (UIProductItem item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
	}
}