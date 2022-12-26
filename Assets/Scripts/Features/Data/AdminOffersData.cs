public class AdminOffersData : AdminDatabaseData
{
	protected override string Path => "offers";

	protected override AdminAttribute[] Attributes { get; } =
	{
		new AdminTitleAttribute("", "Offers"),
		new AdminSearchAttribute(""),
		new AdminCollectionAttribute("", "offer_{0}"),
		new AdminFixedAttribute("{offer_id}/{value}"),
		new AdminTickAttribute("{offer_id}/coins", 1, 10, 100, 1000),
		new AdminTickAttribute("{offer_id}/ads_count", 1),
		new AdminDateAttribute("{offer_id}/timestamp"),
		new AdminLimitAttribute("{offer_id}/coins", 0, decimal.MaxValue),
		new AdminLimitAttribute("{offer_id}/ads_count", 0, 10),
		new AdminHideAttribute("{offer_id}/order"),
	};

	readonly string[] m_Languages;

	public AdminOffersData(params string[] _Languages)
	{
		m_Languages = _Languages;
	}

	public override AdminNode CreateObject(AdminNode _Node, string _Path)
	{
		if (AdminUtility.Match(_Path, "{offer_id}"))
			return CreateOffer(_Node, _Path);
		return null;
	}

	AdminNode CreateOffer(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		AdminNode.Create(this, root, $"{_Path}/active", AdminNodeType.Boolean);
		AdminNode.Create(this, root, $"{_Path}/image", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/coins", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/song_id", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/voucher_id", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/chest_id", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/ads_count", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/timestamp", AdminNodeType.Number);
		
		CreateDescriptors(root, _Path);
		
		return root;
	}

	void CreateDescriptors(AdminNode _Node, string _ID)
	{
		if (m_Languages == null || m_Languages.Length <= 0)
			return;
		
		AdminDescriptorData data = new AdminDescriptorData("offers_descriptors", _ID, m_Languages);
		
		data.Load();
		
		_Node.Attach(data.Root);
	}
}
