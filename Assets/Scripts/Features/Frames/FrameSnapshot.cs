using Firebase.Database;

public class FrameSnapshot : Snapshot
{
	public string Image { get; }

	public FrameSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Image = _Data.GetString("image", $"Thumbnails/Frames/{ID}.png");
	}
}
