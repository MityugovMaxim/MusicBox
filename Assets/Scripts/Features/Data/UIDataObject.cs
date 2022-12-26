using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Zenject;

public class UIDataObject : UIDataEntity
{
	[Preserve]
	public class Pool : UIDataEntityPool<UIDataObject> { }

	[SerializeField] Button m_EditButton;

	[Inject] MenuProcessor m_MenuProcessor;

	protected override void Awake()
	{
		base.Awake();
		
		m_EditButton.Subscribe(Edit);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_EditButton.Subscribe(Edit);
	}

	void Edit()
	{
	}
}
