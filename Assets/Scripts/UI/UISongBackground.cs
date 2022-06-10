public class UISongBackground : UIBackground
{
	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		Show($"Thumbnails/Songs/{m_SongID}.jpg");
	}
}