using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Zenject;

public class UIBackground : UIEntity
{
	[SerializeField] UIBackgroundItem m_Prefab;

	[Inject] UIBackgroundItem.Factory m_Factory;

	readonly Queue<UIBackgroundItem> m_Items = new Queue<UIBackgroundItem>();

	string m_Path;

	CancellationTokenSource m_TokenSource;

	protected async void Show(string _Path, bool _Instant = false)
	{
		if (m_Path == _Path)
			return;
		
		m_Path = _Path;
		
		UIBackgroundItem item = m_Factory.Create(m_Prefab);
		
		if (item == null)
			return;
		
		item.RectTransform.SetParent(RectTransform, false);
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		m_Items.Enqueue(item);
		
		item.Setup(_Path);
		
		await item.ShowAsync(_Instant);
		
		if (token.IsCancellationRequested)
			return;
		
		Clear();
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	void Clear()
	{
		while (m_Items.Count > 1)
			DestroyImmediate(m_Items.Dequeue().gameObject);
	}
}