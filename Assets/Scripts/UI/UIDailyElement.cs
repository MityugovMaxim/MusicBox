using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIDailyElement : UIOverlayButton
{
	[Preserve]
	public class Pool : UIEntityPool<UIDailyElement> { }

	public static bool Processing { get; private set; }

	[SerializeField] UIDailyImage   m_Image;
	[SerializeField] float          m_Duration = 0.2f;
	[SerializeField] AnimationCurve m_Curve    = AnimationCurve.EaseInOut(0, 0, 1, 1);
	[SerializeField] UIDailyItem[]  m_LeftItems;
	[SerializeField] UIDailyItem[]  m_RightItems;
	[SerializeField] UIDailyItem    m_CenterItem;
	[SerializeField] UITimer        m_Timer;
	[SerializeField] UIGroup        m_TimerGroup;
	[SerializeField] UIGroup        m_ItemsGroup;
	[SerializeField] UIGroup        m_LoaderGroup;

	[SerializeField, Sound] string m_Sound;

	[Inject] DailyManager          m_DailyManager;
	[Inject] DailyProcessor        m_DailyProcessor;
	[Inject] MenuProcessor         m_MenuProcessor;
	[Inject] AdsProcessor          m_AdsProcessor;
	[Inject] ProfileProcessor      m_ProfileProcessor;
	[Inject] LocalizationProcessor m_LocalizationProcessor;
	[Inject] MessageProcessor      m_MessageProcessor;
	[Inject] SoundProcessor        m_SoundProcessor;
	[Inject] HapticProcessor       m_HapticProcessor;

	string m_DailyID;

	void Update()
	{
		if (m_Timer != null && !Processing)
			m_Timer.Process();
	}

	public void Setup()
	{
		if (Processing)
			return;
		
		m_DailyID = m_DailyManager.GetDailyID();
		
		m_Image.Setup();
		
		if (m_DailyManager.IsDailyAvailable(m_DailyID))
		{
			ProcessDaily();
			
			m_ItemsGroup.Show(true);
			m_TimerGroup.Hide(true);
		}
		else
		{
			m_DailyID = m_DailyManager.GetDailyIDs().FirstOrDefault();
			
			ProcessDaily();
			
			m_ItemsGroup.Show(true);
			m_TimerGroup.Show(true);
		}
		
		m_Timer.Setup(
			m_DailyManager.GetStartTimestamp(),
			m_DailyManager.GetEndTimestamp(),
			OnTimer
		);
	}

	async void OnTimer()
	{
		await UnityTask.While(() => Processing);
		
		string dailyID = m_DailyManager.GetDailyID();
		
		Processing = true;
		
		m_ProfileProcessor.ProcessTimer();
		
		await m_TimerGroup.HideAsync();
		
		if (m_DailyID != dailyID)
		{
			m_DailyID = dailyID;
			
			await m_ItemsGroup.HideAsync();
			
			ProcessDaily();
			
			await Task.Delay(250);
			
			await m_ItemsGroup.ShowAsync();
		}
		
		Processing = false;
	}

	protected override void OnClick()
	{
		base.OnClick();
		
		Collect();
	}

	void ProcessDaily()
	{
		ProcessPhase(0);
		
		List<string> dailyIDs = m_DailyManager.GetDailyIDs();
		
		int index = dailyIDs.IndexOf(m_DailyID);
		
		int leftIndex = index - 1;
		foreach (UIDailyItem item in m_LeftItems)
		{
			string dailyID = leftIndex >= 0 && leftIndex < dailyIDs.Count ? dailyIDs[leftIndex] : null;
			item.Setup(dailyID);
			leftIndex--;
		}
		
		m_CenterItem.Setup(m_DailyID);
		
		int rightIndex = index + 1;
		foreach (UIDailyItem item in m_RightItems)
		{
			string dailyID = rightIndex >= 0 && rightIndex < dailyIDs.Count ? dailyIDs[rightIndex] : null;
			item.Setup(dailyID);
			rightIndex++;
		}
	}

	Task ShiftAsync()
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		IEnumerator routine = ShiftRoutine(() => completionSource.TrySetResult(true));
		
		StartCoroutine(routine);
		
		return completionSource.Task;
	}

	IEnumerator ShiftRoutine(Action _Finished)
	{
		float time = 0;
		while (time < m_Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			float phase = m_Curve.Evaluate(time / m_Duration);
			
			ProcessPhase(phase);
		}
		
		_Finished?.Invoke();
	}

	void ProcessPhase(float _Phase)
	{
		m_CenterItem.Phase = _Phase;
		foreach (UIDailyItem item in m_LeftItems)
			item.Phase = _Phase;
		foreach (UIDailyItem item in m_RightItems)
			item.Phase = _Phase;
	}

	async void Collect()
	{
		if (Processing || string.IsNullOrEmpty(m_DailyID) || !m_DailyManager.IsDailyAvailable(m_DailyID))
			return;
		
		Processing = true;
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		m_LoaderGroup.Show();
		
		bool success = true;
		
		if (m_DailyProcessor.GetAds(m_DailyID))
		{
			await m_MenuProcessor.Show(MenuType.ProcessingMenu);
			
			success &= await m_AdsProcessor.Rewarded("daily");
			
			await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		}
		
		if (!success)
		{
			Setup();
			
			await m_MenuProcessor.RetryLocalizedAsync(
				"daily_ads",
				"daily_element",
				"DAILY_COLLECT_ERROR_TITLE",
				"COMMON_ERROR_MESSAGE",
				Collect,
				() => { }
			);
			
			await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
			
			m_LoaderGroup.Hide();
			
			Processing = false;
			
			return;
		}
		
		DailyCollectRequest request = new DailyCollectRequest(m_DailyID);
		
		success &= await request.SendAsync();
		
		if (!success)
		{
			Setup();
			
			await m_MenuProcessor.RetryLocalizedAsync(
				"daily_collect",
				"daily_element",
				"DAILY_COLLECT_ERROR_TITLE",
				"COMMON_ERROR_MESSAGE",
				Collect,
				() => { }
			);
			
			await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
			
			m_LoaderGroup.Hide();
			
			Processing = false;
			
			return;
		}
		
		await m_ProfileProcessor.Load();
		
		await m_LoaderGroup.HideAsync();
		
		m_CenterItem.Collect();
		m_SoundProcessor.Play(m_Sound);
		m_HapticProcessor.Process(Haptic.Type.ImpactRigid);
		
		await Task.Delay(500);
		
		string dailyID = m_DailyManager.GetDailyID();
		
		long startTimestamp = m_DailyManager.GetStartTimestamp();
		long endTimestamp   = m_DailyManager.GetEndTimestamp();
		
		if (m_DailyID != dailyID)
		{
			m_MessageProcessor.Schedule(
				"daily",
				Application.productName,
				m_LocalizationProcessor.Get("DAILY_NOTIFICATION"),
				"audiobox://store",
				endTimestamp
			);
			
			m_DailyID = dailyID;
			
			await ShiftAsync();
			
			ProcessDaily();
			
			m_Timer.Setup(startTimestamp, endTimestamp, OnTimer);
		}
		else
		{
			m_Timer.Setup(startTimestamp, endTimestamp, OnTimer);
			
			await Task.WhenAll(
				m_ItemsGroup.HideAsync(),
				m_TimerGroup.ShowAsync()
			);
			
			m_DailyID = m_DailyManager.GetDailyIDs().FirstOrDefault();
			
			ProcessDaily();
			
			await m_ItemsGroup.ShowAsync();
		}
		
		Processing = false;
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}
}






