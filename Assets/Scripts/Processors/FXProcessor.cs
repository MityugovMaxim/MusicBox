using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class FXProcessor : UIEntity
{
	[SerializeField] UIFXHighlight[] m_Highlights;
	[SerializeField] UIFXHighlight   m_Flash;
	[SerializeField] UIFXHighlight   m_Dim;
	[SerializeField] RectTransform   m_InputArea;

	[Inject] UITapFX.Pool    m_TapFXPool;
	[Inject] UIDoubleFX.Pool m_DoubleFXPool;
	[Inject] UIHoldFX.Pool   m_HoldFXPool;

	public async void TapFX(Rect _Rect)
	{
		Highlight(_Rect.center);
		
		UITapFX item = m_TapFXPool.Spawn(RectTransform);
		
		item.RectTransform.localPosition = GetZonePosition(_Rect.center);
		
		await item.PlayAsync();
		
		m_TapFXPool.Despawn(item);
	}

	public async void DoubleFX(Rect _Rect)
	{
		Flash();
		
		UIDoubleFX item = m_DoubleFXPool.Spawn(RectTransform);
		
		item.RectTransform.localPosition = GetZonePosition(_Rect.center);
		
		await item.PlayAsync();
		
		m_DoubleFXPool.Despawn(item);
	}

	public async void HoldFX(Rect _Rect)
	{
		Highlight(_Rect.center);
		
		UIHoldFX item = m_HoldFXPool.Spawn(RectTransform);
		
		item.RectTransform.localPosition = GetZonePosition(_Rect.center);
		
		await item.PlayAsync();
		
		m_HoldFXPool.Despawn(item);
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

	async void Flash()
	{
		await m_Flash.PlayAsync();
	}

	async void Dim()
	{
		await m_Dim.PlayAsync();
	}

	async void Highlight(Vector2 _Position)
	{
		await HighlightAsync(_Position);
	}

	Task HighlightAsync(Vector2 _Position)
	{
		UIFXHighlight highlight = GetHighlight(_Position);
		
		return highlight != null ? highlight.PlayAsync() : null;
	}

	UIFXHighlight GetHighlight(Vector2 _Position)
	{
		return m_Highlights.FirstOrDefault(_Highlight => _Highlight != null && _Highlight.GetWorldRect().Contains(_Position, true));
	}
}