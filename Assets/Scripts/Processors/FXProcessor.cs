using System;
using System.Collections;
using UnityEngine;
using Zenject;

public class FXProcessor : UIEntity, IInitializable
{
	[SerializeField] UIFXHighlight[] m_Highlights;
	[SerializeField] UIFXHighlight   m_Flash;

	UIInputZone     m_InputZone;
	UITapFX.Pool    m_TapFXPool;
	UIDoubleFX.Pool m_DoubleFXPool;
	UIHoldFX.Pool   m_HoldFXPool;

	[Inject]
	public void Construct(
		UIInputZone     _InputZone,
		UITapFX.Pool    _TapFXPool,
		UIDoubleFX.Pool _DoubleFXPool,
		UIHoldFX.Pool   _HoldFXPool
	)
	{
		m_InputZone    = _InputZone;
		m_TapFXPool    = _TapFXPool;
		m_DoubleFXPool = _DoubleFXPool;
		m_HoldFXPool   = _HoldFXPool;
	}

	void IInitializable.Initialize()
	{
		Prewarm();
	}

	public void TapFX(Rect _Rect)
	{
		UITapFX tapFX = m_TapFXPool.Spawn();
		
		tapFX.RectTransform.SetParent(RectTransform, false);
		tapFX.RectTransform.localPosition = GetZonePosition(_Rect.center);
		
		IEnumerator delayRoutine = DelayRoutine(
			tapFX.Duration,
			() => m_TapFXPool.Despawn(tapFX)
		);
		
		StartCoroutine(delayRoutine);
		
		Highlight(_Rect.center);
	}

	public void DoubleFX(Rect _Rect)
	{
		UIDoubleFX doubleFX = m_DoubleFXPool.Spawn();
		
		doubleFX.RectTransform.SetParent(RectTransform, false);
		doubleFX.RectTransform.localPosition = GetZonePosition(_Rect.center);
		
		IEnumerator delayRoutine = DelayRoutine(
			doubleFX.Duration,
			() => m_DoubleFXPool.Despawn(doubleFX)
		);
		
		StartCoroutine(delayRoutine);
		
		Flash();
	}

	public void HoldFX(Rect _Rect)
	{
		UIHoldFX holdFX = m_HoldFXPool.Spawn();
		
		holdFX.RectTransform.SetParent(RectTransform, false);
		holdFX.RectTransform.localPosition = GetZonePosition(_Rect.center);
		
		IEnumerator delayRoutine = DelayRoutine(
			holdFX.Duration,
			() => m_HoldFXPool.Despawn(holdFX)
		);
		
		StartCoroutine(delayRoutine);
		
		Highlight(_Rect.center);
	}

	void Prewarm()
	{
		UITapFX tapFX = m_TapFXPool.Spawn();
		m_TapFXPool.Despawn(tapFX);
		
		UIDoubleFX doubleFX = m_DoubleFXPool.Spawn();
		m_DoubleFXPool.Despawn(doubleFX);
		
		UIHoldFX holdFX = m_HoldFXPool.Spawn();
		m_HoldFXPool.Despawn(holdFX);
	}

	static IEnumerator DelayRoutine(float _Delay, Action _Callback)
	{
		yield return new WaitForSeconds(_Delay);
		
		_Callback?.Invoke();
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

	void Highlight(Vector2 _Position)
	{
		foreach (UIFXHighlight highlight in m_Highlights)
		{
			if (highlight != null && highlight.RectTransform.GetWorldRect().Contains(_Position, true))
				highlight.Play();
		}
	}

	void Flash()
	{
		m_Flash.Play();
	}
}