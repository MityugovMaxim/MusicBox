using System.Collections.Generic;
using System.Linq;

public class UIAdminRoot : UIAdminNode
{
	protected override IReadOnlyList<AdminNode> Children => m_Children;

	readonly List<AdminNode> m_Children = new List<AdminNode>();

	public AdminData[] GetAdminData()
	{
		List<AdminData> data = new List<AdminData>();
		foreach (AdminNode node in Children)
			CollectAdminData(node, data);
		return data.Distinct().ToArray();
	}

	static void CollectAdminData(AdminNode _Node, ICollection<AdminData> _Data)
	{
		if (_Node == null || !_Node.Changed)
			return;
		
		_Data.Add(_Node.Data);
		
		foreach (AdminNode node in _Node.Children)
			CollectAdminData(node, _Data);
	}

	public void Add(AdminNode _Node)
	{
		m_Children.Add(_Node);
	}

	public void Remove(AdminNode _Node)
	{
		m_Children.Remove(_Node);
	}

	public void Clear()
	{
		m_Children.Clear();
	}

	public AdminNode[] GetNodes() => m_Children.ToArray();

	public void Rebuild()
	{
		RefreshNodes();
	}

	protected override void ValueChanged() { }
}
