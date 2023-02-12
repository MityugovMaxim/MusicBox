public class AudioTrack
{
	public string ID     { get; }
	public string Title  { get; }
	public string Artist { get; }
	public string Sound  { get; }

	public AudioTrack(string _ID, string _Title, string _Artist, string _Sound)
	{
		ID     = _ID;
		Title  = _Title;
		Artist = _Artist;
		Sound  = _Sound;
	}
}