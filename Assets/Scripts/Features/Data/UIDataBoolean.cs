using UnityEngine;
using UnityEngine.Scripting;

public class UIDataBoolean : UIDataEntity
{
	[Preserve]
	public class Pool : UIDataEntityPool<UIDataBoolean> { }

	[SerializeField] UIOverlayToggle m_Toggle;

	protected override void Awake()
	{
		base.Awake();
		
		m_Toggle.Subscribe(ProcessValue);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Toggle.Unsubscribe(ProcessValue);
	}

	void ProcessBoolean(bool _Value)
	{
		SetValue(_Value);
	}

	protected override void ProcessValue()
	{
		base.ProcessValue();
		
		m_Toggle.SetState(GetValue<bool>());
	}
}
