using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class TweenCycle
{
	public float          BeforeDelay => m_BeforeDelay;
	public float          AfterDelay  => m_AfterDelay;
	public float          Duration    => m_Duration;
	public AnimationCurve Curve       => m_Curve;

	[SerializeField] float          m_BeforeDelay;
	[SerializeField] float          m_AfterDelay;
	[SerializeField] float          m_Duration;
	[SerializeField] AnimationCurve m_Curve = AnimationCurve.Linear(0, 0, 1, 1);
}

public abstract class Tween<T> : UIEntity
{
	public enum TweenType
	{
		Once,
		Loop,
		PingPong,
	}

	protected T Source => m_Source;
	protected T Target => m_Target;

	[SerializeField] float        m_Delay;
	[SerializeField] TweenCycle[] m_Cycles;
	[SerializeField] TweenType    m_Type;
	[SerializeField] T            m_Source;
	[SerializeField] T            m_Target;
	[SerializeField] bool         m_AutoPlay;

	IEnumerator m_PlayRoutine;

	protected override void OnEnable()
	{
		if (m_PlayRoutine != null)
			StopCoroutine(m_PlayRoutine);
		
		Process(0);
		
		if (m_AutoPlay)
			Play();
	}

	protected override void OnDisable()
	{
		Process(0);
	}

	public void Play()
	{
		Stop();
		
		m_PlayRoutine = PlayRoutine();
		
		StartCoroutine(m_PlayRoutine);
	}

	public void Stop()
	{
		if (m_PlayRoutine != null)
			StopCoroutine(m_PlayRoutine);
		
		m_PlayRoutine = null;
	}

	IEnumerator PlayRoutine()
	{
		if (m_Delay > float.Epsilon)
			yield return new WaitForSeconds(m_Delay);
		
		do
		{
			foreach (TweenCycle cycle in m_Cycles)
			{
				if (cycle.BeforeDelay > float.Epsilon)
					yield return new WaitForSeconds(cycle.BeforeDelay);
				
				Process(0);
				
				float time     = 0;
				float duration = cycle.Duration;
				while (time < duration)
				{
					yield return null;
					
					time += Time.deltaTime;
					
					float phase = m_Type == TweenType.PingPong
						? Mathf.PingPong(time * 2, duration) / duration
						: time / duration;
					
					Process(cycle.Curve.Evaluate(phase));
				}
				
				Process(m_Type == TweenType.PingPong ? 0 : 1);
				
				yield return null;
				
				if (cycle.AfterDelay > float.Epsilon)
					yield return new WaitForSeconds(cycle.AfterDelay);
			}
		}
		while (m_Type != TweenType.Once);
	}

	protected abstract void Process(float _Phase);
}