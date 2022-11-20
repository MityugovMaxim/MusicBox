using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIDailyItem : UIEntity
{
	public string DailyID
	{
		get => m_DailyID;
		set
		{
			if (m_DailyID == value)
				return;
			
			m_DailyManager.UnsubscribeCollect(m_DailyID, CollectDaily);
			m_DailyManager.UnsubscribeRestore(m_DailyID, RestoreDaily);
			m_DailyManager.Collection.Unsubscribe(DataEventType.Change, m_DailyID, ProcessDaily);
			
			m_DailyID = value;
			
			ProcessDaily();
			
			m_DailyManager.SubscribeCollect(m_DailyID, CollectDaily);
			m_DailyManager.SubscribeRestore(m_DailyID, RestoreDaily);
			m_DailyManager.Collection.Subscribe(DataEventType.Change, m_DailyID, ProcessDaily);
		}
	}

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

	[SerializeField] GameObject  m_Ads;
	[SerializeField] UIUnitLabel m_Coins;
	[SerializeField] UIFlare     m_Flare;
	[SerializeField] float       m_Duration;

	[SerializeField] float m_SourceWidth;
	[SerializeField] float m_TargetWidth;
	[SerializeField] float m_CollectDelay;
	[SerializeField] float m_RestoreDelay;

	[SerializeField]        Haptic.Type m_Haptic;
	[SerializeField, Sound] string      m_Sound;

	[Inject] DailyManager    m_DailyManager;
	[Inject] HapticProcessor m_HapticProcessor;
	[Inject] SoundProcessor  m_SoundProcessor;

	CancellationTokenSource m_TokenSource;

	string m_DailyID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		DailyID = null;
		
		m_TokenSource?.Cancel();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (!IsInstanced || Application.isPlaying)
			return;
		
		ProcessPhase();
	}
	#endif

	async void CollectDaily()
	{
		m_TokenSource?.Cancel();
		
		m_TokenSource = new CancellationTokenSource();
		
		m_Flare.Play();
		
		m_HapticProcessor.Process(m_Haptic);
		m_SoundProcessor.Play(m_Sound);
		
		try
		{
			await UnityTask.Phase(
				_Phase => Phase = 1 - _Phase,
				m_CollectDelay,
				m_Duration,
				EaseFunction.EaseOutCubic,
				m_TokenSource.Token
			);
		}
		catch (TaskCanceledException) { }
		catch (OperationCanceledException) { }
		finally
		{
			m_TokenSource.Dispose();
			m_TokenSource = null;
		}
	}

	async void RestoreDaily()
	{
		m_TokenSource?.Cancel();
		
		m_TokenSource = new CancellationTokenSource();
		
		bool available = m_DailyManager.IsDailyAvailable(DailyID);
		
		float source = Phase;
		float target = available ? 1 : 0;
		
		try
		{
			await UnityTask.Phase(
				_Phase => Phase = Mathf.Lerp(source, target, _Phase),
				m_RestoreDelay,
				m_Duration,
				EaseFunction.EaseOutCubic,
				m_TokenSource.Token
			);
		}
		catch (TaskCanceledException) { }
		catch (OperationCanceledException) { }
		finally
		{
			m_TokenSource.Dispose();
			m_TokenSource = null;
		}
	}

	void ProcessDaily()
	{
		bool available = m_DailyManager.IsDailyAvailable(DailyID);
		
		Phase         = available ? 1 : 0;
		
		m_Coins.Value = m_DailyManager.GetCoins(DailyID);
		m_Ads.SetActive(m_DailyManager.IsAds(m_DailyID));
	}

	void ProcessPhase()
	{
		Vector2 size = RectTransform.sizeDelta;
		size.x = Mathf.Lerp(m_SourceWidth, m_TargetWidth, Phase);
		RectTransform.sizeDelta = size;
	}
}
