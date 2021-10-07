using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIMainMenuGroup : UIEntity
{
	[Preserve]
	public class Pool : MonoMemoryPool<UIMainMenuGroup> { }

	[SerializeField] TMP_Text      m_Title;
	[SerializeField] RectTransform m_Container;

	public void Setup(string _Title, IEnumerable<UILevelsPageItem> _Items)
	{
		m_Title.text = _Title;
		
		if (_Items == null)
			return;
		
		foreach (UILevelsPageItem item in _Items)
		{
			if (item != null)
				item.RectTransform.SetParent(m_Container, false);
		}
	}
}