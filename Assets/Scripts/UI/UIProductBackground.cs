public class UIProductBackground : UIBackground
{
	string m_ProductID;

	public void Setup(string _ProductID, bool _Instant = false)
	{
		m_ProductID = _ProductID;
		
		string path = $"Thumbnails/Products/{m_ProductID}.jpg";
		
		Show(path, _Instant);
	}
}