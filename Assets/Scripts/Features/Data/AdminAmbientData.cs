public class AdminAmbientData : AdminDatabaseData
{
	protected override string Path => "ambient";

	protected override AdminAttribute[] Attributes { get; } =
	{
		new AdminTitleAttribute("", "Ambient"),
		new AdminSearchAttribute(""),
		new AdminCollectionAttribute("", "ambient_{0}"),
		new AdminFixedAttribute("{ambient_id}/{value}"),
		new AdminSliderAttribute("{ambient_id}/volume", 0, 1),
	};

	public override AdminNode CreateObject(AdminNode _Node, string _Path)
	{
		if (AdminUtility.Match(_Path, "{ambient_id}"))
			return CreateAmbient(_Node, _Path);
		return null;
	}

	AdminNode CreateAmbient(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		AdminNode.Create(this, root, $"{_Path}/active", AdminNodeType.Boolean);
		AdminNode.Create(this, root, $"{_Path}/title", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/artist", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/sound", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/volume", AdminNodeType.String);
		return root;
	}
}