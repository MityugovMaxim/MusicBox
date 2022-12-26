public class AdminProgressData : AdminDatabaseData
{
	protected override string Path => "progress";

	protected override AdminAttribute[] Attributes { get; } =
	{
		new AdminTitleAttribute("", "Progress"),
		new AdminSearchAttribute(""),
		new AdminCollectionAttribute("", "progress_{0}"),
		new AdminFixedAttribute("{progress_id}/{value}"),
		new AdminTickAttribute("{progress_id}/level", 1, 5, 10),
		new AdminTickAttribute("{progress_id}/discs", 1, 5, 10),
		new AdminTickAttribute("{progress_id}/coins", 1, 10, 100, 1000),
		new AdminMaxAttribute("{progress_id}/level", 0),
		new AdminMaxAttribute("{progress_id}/discs", 0),
		new AdminMaxAttribute("{progress_id}/coins", 0),
		new AdminHideAttribute("{daily_id}/order"),
	};

	public override AdminNode CreateObject(AdminNode _Node, string _Path)
	{
		if (AdminUtility.Match(_Path, "{progress_id}"))
			return CreateProgress(_Node, _Path);
		return null;
	}

	AdminNode CreateProgress(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		AdminNode.Create(this, root, $"{_Path}/active", AdminNodeType.Boolean);
		AdminNode.Create(this, root, $"{_Path}/level", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/discs", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/coins", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/song_id", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/chest_id", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/voucher_id", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/frame_id", AdminNodeType.String);
		return root;
	}
}
