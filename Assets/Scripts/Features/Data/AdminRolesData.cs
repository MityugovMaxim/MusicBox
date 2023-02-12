public class AdminRolesData : AdminDatabaseData
{
	protected override string Path => "roles";

	protected override AdminAttribute[] Attributes { get; } =
	{
		new AdminTitleAttribute("", "Roles"),
		new AdminSearchAttribute(""),
		new AdminCollectionAttribute("", "role_{0}"),
		new AdminFixedAttribute("{role_id}/{value}"),
		new AdminFixedAttribute("{role_id}/permissions/{value}")
	};

	public override AdminNode CreateObject(AdminNode _Node, string _Path)
	{
		if (AdminUtility.Match(_Path, "{role_id}"))
			return CreateRole(_Node, _Path);
		return null;
	}

	AdminNode CreateRole(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		AdminNode.Create(this, root, $"{_Path}/name", AdminNodeType.String);
		
		AdminNode permissions = AdminNode.Create(this, root, $"{_Path}/permissions", AdminNodeType.Object);
		AdminNode.Create(this, permissions, $"{_Path}/permissions/roles", AdminNodeType.Boolean);
		AdminNode.Create(this, permissions, $"{_Path}/permissions/store", AdminNodeType.Boolean);
		AdminNode.Create(this, permissions, $"{_Path}/permissions/products", AdminNodeType.Boolean);
		AdminNode.Create(this, permissions, $"{_Path}/permissions/localization", AdminNodeType.Boolean);
		AdminNode.Create(this, permissions, $"{_Path}/permissions/ambient", AdminNodeType.Boolean);
		AdminNode.Create(this, permissions, $"{_Path}/permissions/songs", AdminNodeType.Boolean);
		AdminNode.Create(this, permissions, $"{_Path}/permissions/difficulty", AdminNodeType.Boolean);
		AdminNode.Create(this, permissions, $"{_Path}/permissions/revives", AdminNodeType.Boolean);
		AdminNode.Create(this, permissions, $"{_Path}/permissions/news", AdminNodeType.Boolean);
		AdminNode.Create(this, permissions, $"{_Path}/permissions/offers", AdminNodeType.Boolean);
		AdminNode.Create(this, permissions, $"{_Path}/permissions/daily", AdminNodeType.Boolean);
		AdminNode.Create(this, permissions, $"{_Path}/permissions/vouchers", AdminNodeType.Boolean);
		AdminNode.Create(this, permissions, $"{_Path}/permissions/seasons", AdminNodeType.Boolean);
		AdminNode.Create(this, permissions, $"{_Path}/permissions/banners", AdminNodeType.Boolean);
		AdminNode.Create(this, permissions, $"{_Path}/permissions/chests", AdminNodeType.Boolean);
		AdminNode.Create(this, permissions, $"{_Path}/permissions/progress", AdminNodeType.Boolean);
		return root;
	}
}
