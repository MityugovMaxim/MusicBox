using System.Collections.Generic;
using Firebase.Database;

public class FrameSnapshot : Snapshot
{
	public string Image { get; }

	public FrameSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Image = _Data.GetString("image", $"Thumbnails/Frames/{ID}.png");
	}

	public override void Serialize(Dictionary<string, object> _Data)
	{
		base.Serialize(_Data);
		
		_Data["image"] = Image;
	}
}