public class AdminStoreData : AdminDatabaseData
{
	protected override string Path => "store";

	protected override AdminAttribute[] Attributes { get; } =
	{
		new AdminTitleAttribute("", "Store"),
		new AdminSearchAttribute(""),
		new AdminCollectionAttribute("", "tier_{0}"),
		new AdminFixedAttribute("{store_id}/{value}")
	};

	public override AdminNode CreateObject(AdminNode _Node, string _Path)
	{
		if (AdminUtility.Match(_Path, "{store_id}"))
			return CreateStoreID(_Node, _Path);
		return null;
	}

	AdminNode CreateStoreID(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		AdminNode.Create(this, root, $"{_Path}/app_store_id", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/google_play_id", AdminNodeType.String);
		return root;
	}
}
