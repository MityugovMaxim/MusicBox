public class UIProductBackground : UIBackground
{
	string m_ProductID;

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		Show($"Thumbnails/Products/{m_ProductID}.jpg");
	}
}