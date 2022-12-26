public class AdminChestsData : AdminDatabaseData
{
	protected override string Path => "chests";

	protected override AdminAttribute[] Attributes { get; } =
	{
		new AdminTitleAttribute("", "Chests"),
		new AdminSearchAttribute(""),
		new AdminCollectionAttribute("", "chest_{0}"),
		new AdminCollectionAttribute("{chest_id}/items", "item_{0}"),
		new AdminFixedAttribute("{chest_id}/{value}"),
		new AdminFixedAttribute("{chest_id}/items/{item_id}/{value}"),
		new AdminEnumAttribute<RankType>("{chest_id}/rank"),
		new AdminEnumAttribute<ChestItemType>("{chest_id}/items/{item_id}/type"),
		new AdminTickAttribute("{chest_id}/items/{item_id}/weight", 1, 10, 100, 1000),
		new AdminTickAttribute("{chest_id}/items/{item_id}/value", 1, 10, 100, 1000),
		new AdminTickAttribute("{chest_id}/boost", 1, 10, 100, 1000),
		new AdminTickAttribute("{chest_id}/capacity", 1),
		new AdminLimitAttribute("{chest_id}/boost", 1, decimal.MaxValue),
		new AdminLimitAttribute("{chest_id}/capacity", 1, decimal.MaxValue),
		new AdminTimeAttribute("{chest_id}/time"),
	};

	public override AdminNode CreateObject(AdminNode _Node, string _Path)
	{
		if (AdminUtility.Match(_Path, "{chest_id}"))
			return CreateChest(_Node, _Path);
		if (AdminUtility.Match(_Path, "{chest_id}/items/{item_id}"))
			return CreateItem(_Node, _Path);
		return null;
	}

	AdminNode CreateChest(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		AdminNode.Create(this, root, $"{_Path}/rank", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/boost", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/time", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/capacity", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/items", AdminNodeType.Object);
		return root;
	}

	AdminNode CreateItem(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		AdminNode.Create(this, root, $"{_Path}/type", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/weight", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/value", AdminNodeType.Number);
		return root;
	}
}
