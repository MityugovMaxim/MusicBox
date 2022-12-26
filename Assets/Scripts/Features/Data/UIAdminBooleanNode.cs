using UnityEngine;

public class UIAdminBooleanNode : UIAdminNode
{
	public class Pool : UIAdminNodePool { }

	[SerializeField] UIOverlayToggle m_Value;

	AdminBooleanNode m_Node;

	protected override void Awake()
	{
		base.Awake();
		
		m_Value.Subscribe(ProcessValue);
	}

	public override void Setup(UIAdminNode _Parent, AdminNode _Node)
	{
		base.Setup(_Parent, _Node);
		
		m_Node = Node as AdminBooleanNode;
		
		ValueChanged();
	}

	protected override void ValueChanged() => m_Value.Value = m_Node?.Value ?? false;

	void ProcessValue(bool _Value)
	{
		if (m_Node != null)
			m_Node.Value = _Value;
	}
}
