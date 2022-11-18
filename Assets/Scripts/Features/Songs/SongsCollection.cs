using UnityEngine.Scripting;

[Preserve]
public class SongsCollection : DataCollection<SongSnapshot>
{
	protected override string Path => "songs";
}
