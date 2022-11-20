using System.Collections.Generic;
using Firebase.Database;

public class Descriptor : Snapshot
{
	public string Title       { get; }
	public string Description { get; }

	public Descriptor(string _ID) : base(_ID, 0)
	{
		Title       = string.Empty;
		Description = string.Empty;
	}

	public Descriptor(DataSnapshot _Data) : base(_Data)
	{
		Title       = _Data.GetString("title");
		Description = _Data.GetString("description");
	}

	public override void Serialize(Dictionary<string, object> _Data)
	{
		base.Serialize(_Data);
		
		_Data["title"]       = Title;
		_Data["description"] = Description;
	}
}
