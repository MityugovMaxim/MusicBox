public class AdminSeasonsData : AdminDatabaseData
{
	protected override string Path => "seasons";

	protected override AdminAttribute[] Attributes { get; } =
	{
		new AdminTitleAttribute("", "Seasons"),
		new AdminSearchAttribute(""),
		new AdminHideAttribute("{season_id}/order"),
		new AdminFixedAttribute("{season_id}/start_timestamp"),
		new AdminFixedAttribute("{season_id}/end_timestamp"),
		new AdminFixedAttribute("{season_id}/levels"),
		new AdminFixedAttribute("{season_id}/levels/{level_id}/level"),
		new AdminFixedAttribute("{season_id}/levels/{level_id}/points"),
		new AdminFixedAttribute("{season_id}/levels/{level_id}/free_item"),
		new AdminFixedAttribute("{season_id}/levels/{level_id}/paid_item"),
		new AdminFixedAttribute("{season_id}/levels/{level_id}/{item_type}/coins"),
		new AdminFixedAttribute("{season_id}/levels/{level_id}/{item_type}/song_id"),
		new AdminFixedAttribute("{season_id}/levels/{level_id}/{item_type}/chest_id"),
		new AdminFixedAttribute("{season_id}/levels/{level_id}/{item_type}/voucher_id"),
		new AdminFixedAttribute("{season_id}/levels/{level_id}/{item_type}/frame_id"),
		new AdminCollectionAttribute("", "season_{0}"),
		new AdminCollectionAttribute("{season_id}/levels", "level_{0}"),
		new AdminDateAttribute("{season_id}/start_timestamp"),
		new AdminDateAttribute("{season_id}/end_timestamp"),
		new AdminTickAttribute("{season_id}/levels/{level_id}/points", 1, 10, 100, 1000),
		new AdminTickAttribute("{season_id}/levels/{level_id}/level", 1, 5),
		new AdminLimitAttribute("{season_id}/levels/{level_id}/level", 1, 100),
		new AdminTickAttribute("{season_id}/levels/{level_id}/free_item/coins", 1, 10, 100, 1000),
		new AdminLimitAttribute("{season_id}/levels/{level_id}/free_item/coins", 0, decimal.MaxValue),
		new AdminTickAttribute("{season_id}/levels/{level_id}/paid_item/coins", 1, 10, 100, 1000),
		new AdminLimitAttribute("{season_id}/levels/{level_id}/paid_item/coins", 0, decimal.MaxValue),
	};

	public override AdminNode CreateObject(AdminNode _Node, string _Path)
	{
		if (AdminUtility.Match(_Path, "{season_id}"))
			return CreateSeason(_Node, _Path);
		if (AdminUtility.Match(_Path, "{season_id}/levels/{level_id}"))
			return CreateLevel(_Node, _Path);
		return null;
	}

	AdminNode CreateSeason(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		AdminNode.Create(this, root, $"{_Path}/start_timestamp", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/end_timestamp", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/levels", AdminNodeType.Object);
		return root;
	}

	AdminNode CreateLevel(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		AdminNode.Create(this, root, $"{_Path}/level", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/points", AdminNodeType.Number);
		CreateItem(root, $"{_Path}/free_item");
		CreateItem(root, $"{_Path}/paid_item");
		return root;
	}

	void CreateItem(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		AdminNode.Create(this, root, $"{_Path}/coins", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/song_id", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/chest_id", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/voucher_id", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/frame_id", AdminNodeType.String);
	}
}
