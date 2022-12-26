public class AdminNewsData : AdminDatabaseData
{
	protected override string Path => "news";

	protected override AdminAttribute[] Attributes { get; } =
	{
		new AdminTitleAttribute("", "News"),
		new AdminSearchAttribute(""),
		new AdminCollectionAttribute("", "news_{0}"),
		new AdminFixedAttribute("{news_id}/{value}"),
		new AdminDateAttribute("{news_id}/timestamp"),
		new AdminHideAttribute("{news_id}/order"),
	};

	readonly string[] m_Languages;

	public AdminNewsData(params string[] _Languages)
	{
		m_Languages = _Languages;
	}

	public override AdminNode CreateObject(AdminNode _Node, string _Path)
	{
		if (AdminUtility.Match(_Path, "{news_id}"))
			return CreateNews(_Node, _Path);
		return null;
	}

	AdminNode CreateNews(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		AdminNode.Create(this, root, $"{_Path}/active", AdminNodeType.Boolean);
		AdminNode.Create(this, root, $"{_Path}/image", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/timestamp", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/url", AdminNodeType.String);
		
		CreateDescriptors(root, _Path);
		
		return root;
	}

	void CreateDescriptors(AdminNode _Node, string _ID)
	{
		if (m_Languages == null || m_Languages.Length <= 0)
			return;
		
		AdminDescriptorData data = new AdminDescriptorData("news_descriptors", _ID, m_Languages);
		
		data.Load();
		
		_Node.Attach(data.Root);
	}
}
