public class AdminSongsData : AdminDatabaseData
{
	protected override string Path => "songs";

	protected override AdminAttribute[] Attributes { get; } =
	{
		new AdminTitleAttribute("", "Songs"),
		new AdminSearchAttribute(""),
		new AdminCollectionAttribute("", "song_{0}"),
		new AdminFixedAttribute("{song_id}/{value}"),
		new AdminEnumAttribute<RankType>("{song_id}/rank"),
		new AdminEnumAttribute<SongMode>("{song_id}/mode"),
		new AdminTickAttribute("{song_id}/price", 1, 10, 100, 1000),
	};

	public override AdminNode CreateObject(AdminNode _Node, string _Path)
	{
		if (AdminUtility.Match(_Path, "{song_id}"))
			return CreateSong(_Node, _Path);
		return null;
	}

	AdminNode CreateSong(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		AdminNode.Create(this, root, $"{_Path}/active", AdminNodeType.Boolean);
		AdminNode.Create(this, root, $"{_Path}/title", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/artist", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/image", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/preview", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/music", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/asf", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/rank", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/mode", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/price", AdminNodeType.Number);
		AdminNode.Create(this, root, $"{_Path}/skin", AdminNodeType.String);
		return root;
	}
}
