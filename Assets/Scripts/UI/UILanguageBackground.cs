public class UILanguageBackground : UIBackground
{
	string m_Language;

	public void Setup(string _Language)
	{
		m_Language = _Language;
		
		Show($"Thumbnails/Languages/{m_Language}.jpg");
	}
}