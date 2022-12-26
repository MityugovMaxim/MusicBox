using System.Text;

public class AdminASFData : AdminStorageData
{
	string SongID { get; }

	protected override string Path => $"Songs/{SongID}.asf";

	protected override Encoding Encoding => Encoding.UTF8;

	protected override AdminAttribute[] Attributes { get; } =
	{
		new AdminTitleAttribute("", "ASF"),
		new AdminFixedAttribute("{track}"),
		new AdminFixedAttribute("{track}/{index}"),
		new AdminFixedAttribute("{track}/{index}/time"),
		new AdminFixedAttribute("{track}/{index}/min_time"),
		new AdminFixedAttribute("{track}/{index}/max_time"),
		new AdminFixedAttribute("{track}/{index}/position"),
		new AdminFixedAttribute("{track}/{index}/keys"),
		new AdminFixedAttribute("{track}/{index}/keys/{index}"),
		new AdminFixedAttribute("{track}/{index}/keys/{index}/time"),
		new AdminFixedAttribute("{track}/{index}/keys/{index}/position"),
		new AdminSliderAttribute("{track}/{index}/position", 0, 1, 4),
		new AdminSliderAttribute("{track}/{index}/keys/{index}/position", 0, 1, 7),
	};

	public AdminASFData(string _SongID)
	{
		SongID = _SongID;
	}

	public override AdminNode CreateObject(AdminNode _Node, string _Path)
	{
		return null;
	}
}
