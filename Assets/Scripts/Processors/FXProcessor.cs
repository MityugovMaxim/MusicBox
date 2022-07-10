using System.Linq;
using UnityEngine;
using Zenject;

public class FXProcessor : UIOrder
{
	[SerializeField] UIFXHighlight[] m_Highlights;
	[SerializeField] UIFXHighlight   m_Flash;
	[SerializeField] UIFXHighlight   m_Dim;
	[SerializeField] RectTransform   m_InputArea;
	[SerializeField] RectTransform   m_FXContainer;

	[Inject(Id = ScoreType.Tap)]    UIIndicatorFX.Pool m_TapFXPool;
	[Inject(Id = ScoreType.Double)] UIIndicatorFX.Pool m_DoubleFXPool;

	public async void TapFX(Rect _Rect, float _Progress)
	{
		Highlight(_Rect.center);
		
		UIIndicatorFX item = m_TapFXPool.Spawn(m_FXContainer);
		
		item.RectTransform.localPosition = GetZonePosition(_Rect.center);
		
		await item.PlayAsync(_Progress);
		
		m_TapFXPool.Despawn(item);
	}

	public async void DoubleFX(Rect _Rect, float _Progress)
	{
		Flash();
		
		UIIndicatorFX item = m_DoubleFXPool.Spawn(m_FXContainer);
		
		item.RectTransform.localPosition = GetZonePosition(_Rect.center);
		
		await item.PlayAsync(_Progress);
		
		m_DoubleFXPool.Despawn(item);
	}

	public async void HoldFX(Rect _Rect, float _Progress)
	{
		Highlight(_Rect.center);
		
		UIIndicatorFX item = m_TapFXPool.Spawn(m_FXContainer);
		
		item.RectTransform.localPosition = GetZonePosition(_Rect.center);
		
		await item.PlayAsync(_Progress);
		
		m_TapFXPool.Despawn(item);
	}

	public void Fail()
	{
		Dim();
	}

	Vector2 GetZonePosition(Vector2 _Position)
	{
		Rect rect = m_InputArea.GetWorldRect();
		
		Vector2 position = new Vector2(
			_Position.x,
			rect.y + rect.height * 0.5f
		);
		
		return RectTransform.InverseTransformPoint(position);
	}

	void Flash()
	{
		m_Flash.Play();
	}

	void Dim()
	{
		m_Dim.Play();
	}

	void Highlight(Vector2 _Position)
	{
		UIFXHighlight highlight = m_Highlights.FirstOrDefault(_Highlight => _Highlight != null && _Highlight.GetWorldRect().Contains(_Position, true));
		
		if (highlight == null)
			return;
		
		highlight.Play();
	}
}