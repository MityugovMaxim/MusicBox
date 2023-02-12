public class AdminBannersData : AdminDatabaseData
{
	protected override string Path => "banners";

	protected override AdminAttribute[] Attributes { get; } =
	{
		new AdminTitleAttribute("", "Banners"),
		new AdminSearchAttribute(""),
		new AdminCollectionAttribute("", "banner_{0}"),
		new AdminFixedAttribute("{banner_id}/{value}"),
		new AdminDateAttribute("{banner_id}/timestamp"),
		new AdminHideAttribute("{banner_id}/order"),
	};

	public override AdminNode CreateObject(AdminNode _Node, string _Path)
	{
		if (AdminUtility.Match(_Path, "{banner_id}"))
			return CreateBanner(_Node, _Path);
		return null;
	}

	AdminNode CreateBanner(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		AdminNode.Create(this, root, $"{_Path}/active", AdminNodeType.Boolean);
		AdminNode.Create(this, root, $"{_Path}/permanent", AdminNodeType.Boolean);
		AdminNode.Create(this, root, $"{_Path}/image", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/language", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/timestamp", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/url", AdminNodeType.String);
		
		return root;
	}
}