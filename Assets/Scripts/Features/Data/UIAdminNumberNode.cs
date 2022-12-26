using System.Globalization;
using UnityEngine;

public class UIAdminNumberNode : UIAdminNode
{
	public class Pool : UIAdminNodePool { }

	[SerializeField] UIAdminField m_Value;

	AdminNumberNode m_Node;

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
		
		m_Node = _Node as AdminNumberNode;
		
		ValueChanged();
	}

	protected override void ValueChanged()
	{
		if (m_Node != null)
			m_Value.Value = m_Node.Value.ToString(CultureInfo.InvariantCulture);
		else
			m_Value.Value = "0";
	}

	void ProcessValue(string _Value)
	{
		if (string.IsNullOrEmpty(_Value))
			return;
		
		if (decimal.TryParse(_Value, out decimal value))
			m_Node.Value = value;
	}
}
