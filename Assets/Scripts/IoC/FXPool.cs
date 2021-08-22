using UnityEngine;
using Zenject;

public class FXPool<T> : MemoryPool<T> where T : Component
{
	Transform m_OriginalParent;

	[Inject]
	public FXPool() { }

	protected override void Reinitialize(T _Item)
	{
		_Item.gameObject.SetActive(false);
		_Item.gameObject.SetActive(true);
	}

	protected override void OnCreated(T _Item)
	{
		m_OriginalParent = _Item.transform.parent;
	}

	protected override void OnDestroyed(T _Item)
	{
		GameObject.Destroy(_Item.gameObject);
	}

	protected override void OnSpawned(T _Item) { }

	protected override void OnDespawned(T _Item)
	{
		if (_Item.transform.parent != m_OriginalParent)
			_Item.transform.SetParent(m_OriginalParent, false);
	}
}