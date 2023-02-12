using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using Zenject;

public class UIBackground : UIEntity
{
	string ID => GetInstanceID().ToString();

	[SerializeField] UIBackgroundItem m_Prefab;

	[Inject] UIBackgroundItem.Factory m_Factory;

	readonly Queue<UIBackgroundItem> m_Items = new Queue<UIBackgroundItem>();

	protected override void OnDisable()
	{
		base.OnDisable();
		
		TokenProvider.CancelToken(this, ID);
		
		while (m_Items.Count > 0)
			Destroy(m_Items.Dequeue().gameObject);
	}

	protected async void Show(string _Path)
	{
		UIBackgroundItem item = m_Factory.Create(m_Prefab);
		
		if (item == null)
			return;
		
		item.RectTransform.SetParent(RectTransform, false);
		
		CancellationToken token = TokenProvider.CreateToken(this, ID);
		
		bool instant = m_Items.Count == 0;
		
		m_Items.Enqueue(item);
		
		item.Setup(_Path);
		
		try
		{
			await item.ShowAsync(instant);
			
			token.ThrowIfCancellationRequested();
			
			Clear();
		}
		catch (TaskCanceledException) { }
		catch (OperationCanceledException) { }
		catch (Exception exception)
		{
			Log.Exception(this, exception);
		}
		finally
		{
			TokenProvider.RemoveToken(this, ID);
		}
	}

	void Clear()
	{
		while (m_Items.Count > 1)
			Destroy(m_Items.Dequeue().gameObject);
	}
}
