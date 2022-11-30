using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class UIChestProgress : UIChestEntity
{
	[SerializeField] TMP_Text         m_Label;
	[SerializeField] UISplineProgress m_Progress;
	[SerializeField] float            m_MinProgress;
	[SerializeField] float            m_MaxProgress;
	[SerializeField] float            m_Duration = 0.5f;
	[SerializeField] AnimationCurve   m_Curve    = AnimationCurve.EaseInOut(0, 0, 1, 1);

	Action m_Finished;

	IEnumerator m_ProgressRoutine;

	public Task<bool> ProgressAsync()
	{
		TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
		
		int   count    = ChestsManager.GetCount(ChestID) + 1;
		int   capacity = ChestsManager.GetCapacity(ChestID);
		float progress = Mathf.InverseLerp(0, capacity, count);
		
		m_Label.text = $"{count}/{capacity}";
		
		SetProgress(progress, false, () => source.TrySetResult(count >= capacity));
		
		return source.Task;
	}

	protected override void Subscribe()
	{
		ChestsManager.Profile.Subscribe(DataEventType.Change, ChestID, ProcessData);
		ChestsManager.Collection.Subscribe(DataEventType.Change, ProcessData);
	}

	protected override void Unsubscribe()
	{
		ChestsManager.Profile.Unsubscribe(DataEventType.Change, ChestID, ProcessData);
		ChestsManager.Collection.Unsubscribe(DataEventType.Change, ProcessData);
	}

	protected override void ProcessData()
	{
		int   count    = ChestsManager.GetCount(ChestID);
		int   capacity = ChestsManager.GetCapacity(ChestID);
		float progress = ChestsManager.GetProgress(ChestID);
		
		m_Label.text = $"{count}/{capacity}";
		
		SetProgress(progress, true);
	}

	void SetProgress(float _Progress, bool _Instant = false, Action _Finished = null)
	{
		InvokeFinished();
		
		if (m_ProgressRoutine != null)
		{
			StopCoroutine(m_ProgressRoutine);
			m_ProgressRoutine = null;
		}
		
		if (gameObject.activeInHierarchy && !_Instant)
		{
			m_ProgressRoutine = ProgressRoutine(_Progress, _Finished);
			StartCoroutine(m_ProgressRoutine);
		}
		else
		{
			m_Progress.Max = Mathf.Lerp(m_MinProgress, m_MaxProgress, _Progress);
		}
	}

	IEnumerator ProgressRoutine(float _Progress, Action _Finished)
	{
		float source = m_Progress.Max;
		float target = Mathf.Lerp(m_MinProgress, m_MaxProgress, _Progress);
		
		if (!Mathf.Approximately(source, target))
		{
			float time = 0;
			while (time < m_Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				float phase = m_Curve.Evaluate(time / m_Duration);
				
				m_Progress.Max = Mathf.Lerp(source, target, phase);
			}
		}
		
		m_Progress.Max = target;
		
		_Finished?.Invoke();
	}

	void InvokeFinished()
	{
		Action action = m_Finished;
		m_Finished = null;
		action?.Invoke();
	}
}
