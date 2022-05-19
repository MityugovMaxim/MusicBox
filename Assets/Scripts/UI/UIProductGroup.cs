using System.Collections.Generic;
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

	[SerializeField] RectTransform m_Container;
	
	[Inject] UIProductItem.Pool m_ItemPool;

	readonly List<UIProductItem> m_Items = new List<UIProductItem>();

	public void Setup(ICollection<string> _ProductIDs)
	{
		Clear();
		
		Fill(_ProductIDs);
	}

	void Fill(ICollection<string> _ProductIDs)
	{
		if (_ProductIDs == null || _ProductIDs.Count == 0)
			return;
		
		foreach (string productID in _ProductIDs)
		{
			if (string.IsNullOrEmpty(productID))
				continue;
			
			UIProductItem item = m_ItemPool.Spawn(m_Container);
			
			if (item == null)
				continue;
			
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