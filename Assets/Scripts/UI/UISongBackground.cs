public class UISongBackground : UIBackground
{
	string m_SongID;

	public void Setup(string _SongID, bool _Instant = false)
	{
		m_SongID = _SongID;
		
		string path = $"Thumbnails/Songs/{m_SongID}.jpg";
		
		Show(path, _Instant);
	}
}