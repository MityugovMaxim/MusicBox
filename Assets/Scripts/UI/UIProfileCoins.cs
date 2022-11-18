using System;
using System.Collections;
using UnityEngine;
using Zenject;

public class UIProfileCoins : UIEntity
{
	[SerializeField] UIUnitLabel m_Coins;
	[SerializeField] float       m_Duration = 0.4f;

	[Inject] CoinsParameter m_CoinsParameter;

	IEnumerator m_CoinsRoutine;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		SetCoins(m_CoinsParameter.Value, true);
		
		m_CoinsParameter.Subscribe(ProcessCoins);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_CoinsParameter.Unsubscribe(ProcessCoins);
	}

	void ProcessCoins(long _Coins) => SetCoins(_Coins);

	void SetCoins(long _Coins, bool _Instant = false)
	{
		if (m_CoinsRoutine != null)
		{
			StopCoroutine(m_CoinsRoutine);
			m_CoinsRoutine = null;
		}
		
		if (gameObject.activeInHierarchy && !_Instant)
		{
			m_CoinsRoutine = CoinsRoutine(_Coins);
			StartCoroutine(m_CoinsRoutine);
		}
		else
		{
			m_Coins.Value = _Coins;
		}
	}

	IEnumerator CoinsRoutine(long _Coins)
	{
		long source = (long)m_Coins.Value;
		long target = Math.Max(0, _Coins);
		
		if (source != target)
		{
			float time = 0;
			while (time < m_Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				m_Coins.Value = MathUtility.Lerp(source, target, time / m_Duration);
			}
		}
		
		m_Coins.Value = target;
	}
}
