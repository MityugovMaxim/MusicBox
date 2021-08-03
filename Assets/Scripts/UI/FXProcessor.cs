using UnityEngine;
using Zenject;

public class FXProcessor : UIEntity
{
	// TODO: Make pool

	[SerializeField] GameObject m_HoldFX;
	[SerializeField] GameObject m_TapFX;
	[SerializeField] GameObject m_DoubleFX;

	UIInputZone m_InputZone;

	[Inject]
	public void Construct(UIInputZone _InputZone)
	{
		m_InputZone = _InputZone;
	}

	public void HoldFX(Rect _Rect)
	{
		GameObject holdFX = Instantiate(m_HoldFX, RectTransform);
		
		holdFX.transform.localPosition = GetZonePosition(_Rect.center);
		
		Destroy(holdFX, 2);
	}

	public void TapFX(Rect _Rect)
	{
		GameObject tapFX = Instantiate(m_TapFX, RectTransform);
		
		tapFX.transform.localPosition = GetZonePosition(_Rect.center);
		
		Destroy(tapFX, 2);
	}

	public void DoubleFX(Rect _Rect)
	{
		GameObject doubleFX = Instantiate(m_DoubleFX, RectTransform);
		
		doubleFX.transform.localPosition = GetZonePosition(_Rect.center);
		
		Destroy(doubleFX, 2);
	}

	Vector2 GetZonePosition(Vector2 _Position)
	{
		Rect rect = m_InputZone.GetWorldRect();
		
		Vector2 position = new Vector2(
			_Position.x,
			rect.y + rect.height * 0.5f
		);
		
		return RectTransform.InverseTransformPoint(position);
	}
}