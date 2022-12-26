using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

public class UIDataCollection : UIDataEntity
{
	[Preserve]
	public class Pool : UIDataEntityPool<UIDataCollection> { }

	IDataNodeCollection Collection => DataNode as IDataNodeCollection;

	[SerializeField] Button m_AddButton;

	protected override void Awake()
	{
		base.Awake();
		
		m_AddButton.Subscribe(Add);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_AddButton.Unsubscribe(Add);
	}

	void Add() => Collection?.Add();
}
