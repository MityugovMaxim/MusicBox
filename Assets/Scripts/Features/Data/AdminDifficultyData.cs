public class AdminDifficultyData : AdminDatabaseData
{
	protected override string Path => "difficulty";

	protected override AdminAttribute[] Attributes { get; } =
	{
		new AdminTitleAttribute("", "Difficulty"),
		new AdminSearchAttribute(""),
		new AdminCollectionAttribute("", "difficulty_{0}"),
		new AdminEnumAttribute<RankType>("{difficulty_id}/type"),
		new AdminSliderAttribute("{difficulty_id}/speed", 400, 1150, 4),
		new AdminSliderAttribute("{difficulty_id}/input_offset", -100, 100, 201),
		new AdminSliderAttribute("{difficulty_id}/input_extend", -100, 100, 201),
		new AdminSliderAttribute("{difficulty_id}/{rank}/threshold", 0, 100, 101),
		new AdminFixedAttribute("{difficulty_id}/{value}"),
		new AdminFixedAttribute("{difficulty_id}/{rank}/{value}"),
		new AdminTickAttribute("{difficulty_id}/{rank}/coins", 1, 10, 100, 1000),
		new AdminTickAttribute("{difficulty_id}/{rank}/points", 1, 10, 100, 1000),
		new AdminLimitAttribute("{difficulty_id}/{rank}/coins", 0, decimal.MaxValue),
		new AdminLimitAttribute("{difficulty_id}/{rank}/points", 0, decimal.MaxValue),
	};

	public override AdminNode CreateObject(AdminNode _Node, string _Path)
	{
		if (AdminUtility.Match(_Path, "{difficulty_id}"))
			return CreateDifficulty(_Node, _Path);
		return null;
	}

	AdminNode CreateDifficulty(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		AdminNode.Create(this, root, $"{_Path}/type", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/speed", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/input_offset", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/input_extend", AdminNodeType.Number);
		CreateRank(root, $"{_Path}/none");
		CreateRank(root, $"{_Path}/bronze");
		CreateRank(root, $"{_Path}/silver");
		CreateRank(root, $"{_Path}/gold");
		CreateRank(root, $"{_Path}/platinum");
		return root;
	}

	void CreateRank(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		AdminNode.Create(this, root, $"{_Path}/threshold", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/coins", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/points", AdminNodeType.Number);
	}
}
