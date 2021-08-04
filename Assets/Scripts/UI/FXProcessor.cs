using System;
using System.Collections;
using UnityEngine;
using Zenject;

public class FXProcessor : UIEntity
{
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
}