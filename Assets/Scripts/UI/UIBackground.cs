using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Zenject;

public class UIBackground : UIEntity
{
	[SerializeField] UIBackgroundItem m_Prefab;

	[Inject] UIBackgroundItem.Factory m_Factory;

	readonly Queue<UIBackgroundItem> m_Items = new Queue<UIBackgroundItem>();

	CancellationTokenSource m_TokenSource;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		while (m_Items.Count > 0)
			Destroy(m_Items.Dequeue().gameObject);
	}

	protected async void Show(string _Path)
	{
		UIBackgroundItem item = m_Factory.Create(m_Prefab);
		
		if (item == null)
			return;
		
		item.RectTransform.SetParent(RectTransform, false);
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		bool instant = m_Items.Count == 0;
		
		m_Items.Enqueue(item);
		
		item.Setup(_Path);
		
		await item.ShowAsync(instant);
		
		if (token.IsCancellationRequested)
			return;
		
		Clear();
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	void Clear()
	{
		while (m_Items.Count > 1)
			Destroy(m_Items.Dequeue().gameObject);
	}
}
