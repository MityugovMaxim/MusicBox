using System.Collections.Generic;
using System.Linq;
using Firebase.Database;

public class ProfileFrame : Snapshot
{
	public string       FrameID  { get; }
	public List<string> FrameIDs { get; }

	public ProfileFrame(DataSnapshot _Data) : base(_Data)
	{
		FrameID  = _Data.GetString("frame_id");
		FrameIDs = _Data.GetChildKeys("frame_ids");
	}
}
