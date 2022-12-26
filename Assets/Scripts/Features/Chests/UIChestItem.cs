using UnityEngine;

public class UIChestItem : UIEntity
{
	[SerializeField] UIChestImage m_Image;

	public void Setup(string _ChestID)
	{
		m_Image.ChestID = _ChestID;
	}
}
