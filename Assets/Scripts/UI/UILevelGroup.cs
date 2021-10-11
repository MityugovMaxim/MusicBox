using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UILevelGroup : UIEntity
{
	[Preserve]
	public class Pool : MonoMemoryPool<UILevelGroup> { }

	[SerializeField] TMP_Text      m_Title;
	[SerializeField] RectTransform m_Container;

	public void Setup(string _Title, IEnumerable<UILevelItem> _Items)
	{
		m_Title.text = _Title;
		
		if (_Items == null)
			return;
		
		foreach (UILevelItem item in _Items)
		{
			if (item != null)
				item.RectTransform.SetParent(m_Container, false);
		}
	}
}