using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UIResultRank : UIEntity
{
	[SerializeField] float      m_Duration;
	[SerializeField] Color      m_SourceColor;
	[SerializeField] Color      m_TargetColor;
	[SerializeField] UIDisc     m_Disc;
	[SerializeField] GameObject m_FX;

	IEnumerator m_ColorRoutine;

	public void Setup(ScoreRank _Rank)
	{
		m_FX.SetActive(false);
		
		m_Disc.Color = m_SourceColor;
		
		Unlock(_Rank, true);
	}

	public Task Unlock(ScoreRank _Rank, bool _Instant = false, CancellationToken _Token = default)
	{
		TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
		
		if (_Rank < m_Disc.Rank)
		{
			taskSource.SetResult(false);
			return taskSource.Task;
		}
		
		void UnlockFinished()
		{
			if (m_ColorRoutine != null)
				StopCoroutine(m_ColorRoutine);
			
			m_FX.SetActive(false);
			
			taskSource.SetResult(true);
		}
		
		if (m_ColorRoutine != null)
			StopCoroutine(m_ColorRoutine);
		
		if (_Token.IsCancellationRequested)
		{
			UnlockFinished();
			return taskSource.Task;
		}
		
		_Token.Register(UnlockFinished);
		
		if (_Instant || !gameObject.activeInHierarchy)
		{
			m_Disc.Color = m_TargetColor;
			
			UnlockFinished();
		}
		else
		{
			m_ColorRoutine = ColorRoutine(m_Disc, m_TargetColor, m_Duration, UnlockFinished);
			
			m_FX.SetActive(true);
			
			StartCoroutine(m_ColorRoutine);
		}
		
		return taskSource.Task;
	}

	static IEnumerator ColorRoutine(UIDisc _Disc, Color _Color, float _Duration, Action _Callback)
	{
		if (_Disc == null)
		{
			_Callback?.Invoke();
			yield break;
		}
		
		Color source = _Disc.Color;
		Color target = _Color;
		
		if (source != target)
		{
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				_Disc.Color = Color.Lerp(source, target, time / _Duration);
			}
		}
		
		_Disc.Color = target;
		
		_Callback?.Invoke();
	}
}