using UnityEngine;

public class PurchaseThumbnail : Thumbnail
{
	public override string ID => m_ID;

	[SerializeField] string m_ID;
}