public class UILanguageBackground : UIBackground
{
	string m_Language;

	public void Setup(string _Language, bool _Instant = false)
	{
		m_Language = _Language;
		
		string path = $"Thumbnails/Languages/{m_Language}.jpg";
		
		Show(path, _Instant);
	}
}