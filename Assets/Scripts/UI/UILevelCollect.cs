using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UILevelCollect : UIEntity
{
	[SerializeField] UISplinePath   m_Path;
	[SerializeField] UIDisc         m_Disc;
	[SerializeField] UIFlare        m_Flare;
	[SerializeField] UISpline[]     m_Splines;
	[SerializeField] float          m_Duration;
	[SerializeField] AnimationCurve m_Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

	CancellationTokenSource m_TokenSource;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		Cancel();
	}

	public async Task CollectAsync(ScoreRank _Rank)
	{
		Cancel();
		
		int index = Random.Range(0, m_Splines.Length);
		
		m_Disc.Rank   = _Rank;
		m_Path.Phase  = 0;
		m_Path.Spline = m_Splines[index];
		
		m_Flare.Play(m_Duration - 0.1f);
		
		try
		{
			await UnityTask.Phase(
				_Phase => m_Path.Phase = _Phase,
				m_Duration,
				m_Curve
			);
			
			await Task.Yield();
		}
		catch (TaskCanceledException)
		{
			return;
		}
		
		m_Path.Phase = 0;
		m_Disc.Rank  = ScoreRank.None;
		
		Complete();
	}

	void Cancel()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	void Complete()
	{
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}
}