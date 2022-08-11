using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class UILevelProgress : UIEntity
{
	[SerializeField] UISplineLine m_Progress;
	[SerializeField] TMP_Text     m_Label;
	[SerializeField] float        m_MinProgress;
	[SerializeField] float        m_MaxProgress;
	[SerializeField] float        m_Duration;

	int m_Discs;
	int m_SourceDiscs;
	int m_TargetDiscs;

	CancellationTokenSource m_TokenSource;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		Cancel();
	}

	public void Setup(int _Discs, int _SourceDiscs, int _TargetDiscs)
	{
		m_Discs       = _Discs;
		m_SourceDiscs = _SourceDiscs;
		m_TargetDiscs = _TargetDiscs;
		
		ProcessLabel();
		
		float progress = Mathf.InverseLerp(m_SourceDiscs, m_TargetDiscs, m_Discs);
		
		m_Progress.Max = Mathf.Lerp(m_MinProgress, m_MaxProgress, progress);
	}

	public async Task IncrementAsync()
	{
		Cancel();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		m_Discs++;
		
		try
		{
			float progress = Mathf.InverseLerp(m_SourceDiscs, m_TargetDiscs, m_Discs);
			float source   = m_Progress.Max;
			float target   = Mathf.Lerp(m_MinProgress, m_MaxProgress, progress);
			await UnityTask.Phase(
				_Phase => m_Progress.Max = Mathf.Lerp(source, target, _Phase),
				m_Duration,
				EaseFunction.EaseOut,
				token
			);
		}
		catch (TaskCanceledException)
		{
			return;
		}
		finally
		{
			ProcessLabel();
		}
		
		Complete();
	}

	void ProcessLabel()
	{
		int source = Mathf.Max(0, m_Discs - m_SourceDiscs);
		int target = Mathf.Max(0, m_TargetDiscs - m_SourceDiscs);
		
		m_Label.text = $"{source}/{target}";
	}

	void Complete()
	{
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	void Cancel()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}
}