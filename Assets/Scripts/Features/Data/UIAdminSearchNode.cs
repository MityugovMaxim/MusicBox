using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UIAdminSearchNode : UIAdminNode
{
	public class Pool : UIAdminNodePool { }

	[SerializeField] UIAdminField m_SearchField;
	[SerializeField] Button       m_ClearButton;

	protected override void Awake()
	{
		base.Awake();
		
		m_SearchField.Subscribe(ProcessSearch);
		m_ClearButton.Subscribe(Clear);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_SearchField.Unsubscribe(ProcessSearch);
		m_ClearButton.Unsubscribe(Clear);
	}

	protected override void ValueChanged() => RefreshNodes();

	protected override bool FilterNode(AdminNode _Node)
	{
		if (_Node == null)
			return false;
		
		string filter = GetFilter(m_SearchField.Value);
		
		if (string.IsNullOrEmpty(filter) || filter.Length < 3)
			return base.FilterNode(_Node);
		
		string node = GetFilter(_Node.Name);
		
		if (string.IsNullOrEmpty(node))
			return false;
		
		return node.Contains(filter) && base.FilterNode(_Node);
	}

	static string GetFilter(string _Value)
	{
		if (string.IsNullOrEmpty(_Value))
			return null;
		
		StringBuilder builder = new StringBuilder();
		foreach (char symbol in _Value)
		{
			if (char.IsLetterOrDigit(symbol))
				builder.Append(char.ToLowerInvariant(symbol));
		}
		return builder.ToString();
	}

	void ProcessSearch(string _Value)
	{
		RefreshNodes();
	}

	void Clear()
	{
		m_SearchField.Value = string.Empty;
		
		RefreshNodes();
	}
}
