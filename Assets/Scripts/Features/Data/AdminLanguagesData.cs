using System.Linq;

public class AdminLanguagesData : AdminDatabaseData
{
	protected override string Path => "languages";

	public string[] Languages => Root.Children.Select(_Node => _Node.Name).ToArray();

	protected override AdminAttribute[] Attributes { get; } =
	{
		new AdminTitleAttribute("", "Languages"),
		new AdminSearchAttribute(""),
		new AdminCollectionAttribute("", "language_code_{0}"),
		new AdminHideAttribute("{language_code}/order"),
		new AdminFixedAttribute("{language_code}/{value}"),
	};

	public override AdminNode CreateObject(AdminNode _Node, string _Path)
	{
		if (AdminUtility.Match(_Path, "{language_code}"))
			return CreateLanguage(_Node, _Path);
		return null;
	}

	AdminNode CreateLanguage(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		AdminNode.Create(this, root, $"{_Path}/active", AdminNodeType.Boolean);
		AdminNode.Create(this, root, $"{_Path}/name", AdminNodeType.String);
		return root;
	}
}
