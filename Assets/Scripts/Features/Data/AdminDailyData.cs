public class AdminDailyData : AdminDatabaseData
{
	protected override string Path => "daily";

	protected override AdminAttribute[] Attributes { get; } =
	{
		new AdminTitleAttribute("", "Daily"),
		new AdminCollectionAttribute("", "daily_{0}"),
		new AdminFixedAttribute("{daily_id}/{value}"),
		new AdminTimeAttribute("{daily_id}/cooldown"),
		new AdminTickAttribute("{daily_id}/coins", 1, 10, 100, 1000),
		new AdminMaxAttribute("{daily_id}/coins", 0),
		new AdminHideAttribute("{daily_id}/order"),
	};

	public override AdminNode CreateObject(AdminNode _Node, string _Path)
	{
		if (AdminUtility.Match(_Path, "{daily_id}"))
			return CreateDaily(_Node, _Path);
		return null;
	}

	AdminNode CreateDaily(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		AdminNode.Create(this, root, $"{_Path}/active", AdminNodeType.Boolean);
		AdminNode.Create(this, root, $"{_Path}/ads", AdminNodeType.Boolean);
		AdminNode.Create(this, root, $"{_Path}/cooldown", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/coins", AdminNodeType.Number);
		return root;
	}
}
