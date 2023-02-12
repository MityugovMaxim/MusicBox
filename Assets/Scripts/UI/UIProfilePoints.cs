using System;
using System.Collections;
using UnityEngine;
using Zenject;

public class UIProfilePoints : UIEntity
{
	[SerializeField] UIUnitLabel m_Points;
	[SerializeField] float       m_Duration = 0.4f;

	[Inject] ProfilePointsParameter m_ProfilePoints;

	IEnumerator m_PointsRoutine;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		SetPoints(m_ProfilePoints.Value, true);
		
		m_ProfilePoints.Subscribe(ProcessCoins);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_ProfilePoints.Unsubscribe(ProcessCoins);
	}

	void ProcessCoins(long _Coins) => SetPoints(_Coins);

	void SetPoints(long _Points, bool _Instant = false)
	{
		if (m_PointsRoutine != null)
		{
			StopCoroutine(m_PointsRoutine);
			m_PointsRoutine = null;
		}
		
		if (gameObject.activeInHierarchy && !_Instant)
		{
			m_PointsRoutine = CoinsRoutine(_Points);
			StartCoroutine(m_PointsRoutine);
		}
		else
		{
			m_Points.Value = _Points;
		}
	}

	IEnumerator CoinsRoutine(long _Points)
	{
		long source = (long)m_Points.Value;
		long target = Math.Max(0, _Points);
		
		if (source != target)
		{
			float time = 0;
			while (time < m_Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				m_Points.Value = MathUtility.Lerp(source, target, time / m_Duration);
			}
		}
		
		m_Points.Value = target;
	}
}