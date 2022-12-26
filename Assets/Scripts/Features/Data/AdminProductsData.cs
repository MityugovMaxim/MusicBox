public class AdminProductsData : AdminDatabaseData
{
	protected override string Path => "products";

	protected override AdminAttribute[] Attributes { get; } =
	{
		new AdminTitleAttribute("", "Products"),
		new AdminSearchAttribute(""),
		new AdminCollectionAttribute("", "product_{0}"),
		new AdminCollectionAttribute("{product_id}/song_ids", "song_{0}"),
		new AdminFixedAttribute("{product_id}/{value}"),
		new AdminEnumAttribute<ProductType>("{product_id}/type"),
		new AdminTickAttribute("{product_id}/coins", 1, 10, 100, 1000),
		new AdminMaxAttribute("{product_id}/coins", 0),
	};

	public override AdminNode CreateObject(AdminNode _Node, string _Path)
	{
		if (AdminUtility.Match(_Path, "{product_id}"))
			return CreateProduct(_Node, _Path);
		if (AdminUtility.Match(_Path, "{product_id}/song_ids/{song_id}"))
			return CreateSongID(_Node, _Path);
		return null;
	}

	AdminNode CreateProduct(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		AdminNode.Create(this, root, $"{_Path}/active", AdminNodeType.Boolean);
		AdminNode.Create(this, root, $"{_Path}/image", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/store_id", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/type", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/coins", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/season_id", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/song_ids", AdminNodeType.Object);
		return root;
	}

	AdminNode CreateSongID(AdminNode _Node, string _Path)
	{
		return AdminNode.Create(this, _Node, _Path, AdminNodeType.Boolean);
	}
}
