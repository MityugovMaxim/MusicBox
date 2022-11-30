using System.Collections;
using UnityEngine;
using Zenject;

public class UIDailyItem : UIDailyEntity
{
	float Phase
	{
		get => m_Phase;
		set
		{
			if (Mathf.Approximately(m_Phase, value))
				return;
			
			m_Phase = value;
			
			ProcessPhase();
		}
	}

	[SerializeField, Range(0, 1)] float m_Phase;

	[SerializeField] UIDailyCoins m_Coins;
	[SerializeField] UIDailyAds   m_Ads;

	[SerializeField] UIFlare m_Flare;
	[SerializeField] float   m_Duration;

	[SerializeField] float m_SourceWidth;
	[SerializeField] float m_TargetWidth;
	[SerializeField] float m_CollectDelay;
	[SerializeField] float m_RestoreDelay;

	[SerializeField]        Haptic.Type m_Haptic;
	[SerializeField, Sound] string      m_Sound;

	[Inject] HapticProcessor m_HapticProcessor;
	[Inject] SoundProcessor  m_SoundProcessor;

	IEnumerator m_ToggleRoutine;

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (!IsInstanced || Application.isPlaying)
			return;
		
		ProcessPhase();
	}
	#endif

	public override void Subscribe()
	{
		DailyManager.SubscribeCollect(DailyID, CollectDaily);
		DailyManager.SubscribeRestore(DailyID, RestoreDaily);
	}

	public override void Unsubscribe()
	{
		DailyManager.UnsubscribeCollect(DailyID, CollectDaily);
		DailyManager.UnsubscribeRestore(DailyID, RestoreDaily);
	}

	public override void ProcessData()
	{
		m_Coins.DailyID = DailyID;
		m_Ads.DailyID   = DailyID;
		
		Phase = DailyManager.IsDailyAvailable(DailyID) ? 1 : 0;
		
		ProcessPhase();
	}

	void CollectDaily()
	{
		m_Flare.Play();
		
		m_HapticProcessor.Process(m_Haptic);
		m_SoundProcessor.Play(m_Sound);
		
		Toggle(false);
	}

	void RestoreDaily()
	{
		Toggle(true);
	}

	void ProcessPhase()
	{
		Vector2 size = RectTransform.sizeDelta;
		size.x                  = Mathf.Lerp(m_SourceWidth, m_TargetWidth, Phase);
		RectTransform.sizeDelta = size;
	}

	void Toggle(bool _Value, bool _Instant = false)
	{
		if (m_ToggleRoutine != null)
		{
			StopCoroutine(m_ToggleRoutine);
			m_ToggleRoutine = null;
		}
		
		float phase = _Value ? 1 : 0;
		float delay = _Value ? m_RestoreDelay : m_CollectDelay;
		
		if (gameObject.activeInHierarchy && !_Instant)
		{
			m_ToggleRoutine = ToggleRoutine(phase, delay);
			StartCoroutine(m_ToggleRoutine);
		}
		else
		{
			Phase = phase;
		}
	}

	IEnumerator ToggleRoutine(float _Phase, float _Delay)
	{
		float source = Phase;
		float target = Mathf.Clamp01(_Phase);
		
		if (_Delay > float.Epsilon)
			yield return new WaitForSeconds(_Delay);
		
		if (!Mathf.Approximately(source, target))
		{
			float time = 0;
			while (time < m_Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				Phase = Mathf.Lerp(source, target, EaseFunction.EaseOutCubic.Get(time / m_Duration));
			}
		}
		
		Phase = target;
	}
}
