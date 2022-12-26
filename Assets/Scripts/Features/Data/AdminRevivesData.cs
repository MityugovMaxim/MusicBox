public class AdminRevivesData : AdminDatabaseData
{
	protected override string Path => "revives";

	protected override AdminAttribute[] Attributes { get; } =
	{
		new AdminTitleAttribute("", "Revives"),
		new AdminSearchAttribute(""),
		new AdminCollectionAttribute("", "revive_{0}"),
		new AdminTickAttribute("{revive_id}/count", 1, 5, 10),
		new AdminTickAttribute("{revive_id}/coins", 1, 10, 100, 1000),
		new AdminMaxAttribute("{revive_id}/count", 0),
		new AdminMaxAttribute("{revive_id}/coins", 0),
		new AdminFixedAttribute("{revive_id}/{value}"),
		new AdminHideAttribute("{daily_id}/order"),
	};

	public override AdminNode CreateObject(AdminNode _Node, string _Path)
	{
		if (AdminUtility.Match(_Path, "{revive_id}"))
			return CreateRevive(_Node, _Path);
		return null;
	}

	AdminNode CreateRevive(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		AdminNode.Create(this, root, $"{_Path}/active", AdminNodeType.Boolean);
		AdminNode.Create(this, root, $"{_Path}/ads", AdminNodeType.Boolean);
		AdminNode.Create(this, root, $"{_Path}/count", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/coins", AdminNodeType.Number);
		return root;
	}
}
