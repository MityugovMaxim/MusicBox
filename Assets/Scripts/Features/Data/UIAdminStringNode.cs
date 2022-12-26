using UnityEngine;

public class UIAdminStringNode : UIAdminNode
{
	public class Pool : UIAdminNodePool { }

	[SerializeField] UIAdminField m_Value;

	AdminStringNode m_Node;

	protected override void Awake()
	{
		base.Awake();
		
		m_Value.Subscribe(ProcessValue);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Value.Unsubscribe(ProcessValue);
	}

	public override void Setup(UIAdminNode _Parent, AdminNode _Node)
	{
		base.Setup(_Parent, _Node);
		
		m_Node = _Node as AdminStringNode;
		
		ValueChanged();
	}

	protected override void ValueChanged()
	{
		if (m_Node != null)
			m_Value.Value = m_Node.Value;
		else
			m_Value.Value = string.Empty;
	}

	void ProcessValue(string _Value)
	{
		m_Node.Value = _Value ?? string.Empty;
	}
}
