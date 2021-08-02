using UnityEngine;

public class FXProcessor : UIEntity
{
	[SerializeField] RectTransform m_Zone;
	[SerializeField] GameObject    m_HoldFX;
	[SerializeField] GameObject    m_TapFX;
	[SerializeField] GameObject    m_DoubleFX;

	public void HoldFX(Rect _Rect)
	{
		GameObject holdFX = Instantiate(m_HoldFX, RectTransform);
		
		holdFX.transform.localPosition = GetZonePosition(_Rect.center);
		
		Destroy(holdFX, 2);
	}

	public void TapFX(Rect _Rect)
	{
		
	}

	public void DoubleFX(Rect _Rect)
	{
		
	}

	Vector2 GetZonePosition(Vector2 _Position)
	{
		Rect rect = m_Zone.GetWorldRect();
		
		Vector2 position = new Vector2(
			_Position.x,
			rect.y + rect.height * 0.5f
		);
		
		return RectTransform.InverseTransformPoint(position);
	}
}