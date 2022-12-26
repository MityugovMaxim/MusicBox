using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UILevelCollect : UIEntity
{
	[SerializeField] UISplinePath   m_Path;
	[SerializeField] UIDisc         m_Disc;
	[SerializeField] UIFlare        m_Flare;
	[SerializeField] UISpline[]     m_Splines;
	[SerializeField] float          m_Duration;
	[SerializeField] AnimationCurve m_Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

	[SerializeField, Sound] string m_FlySound;
	[SerializeField, Sound] string m_CollectSound;

	[Inject] SoundProcessor  m_SoundProcessor;
	[Inject] HapticProcessor m_HapticProcessor;

	CancellationTokenSource m_TokenSource;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		Cancel();
	}

	public async Task CollectAsync(RankType _Rank)
	{
		Cancel();
		
		int index = Random.Range(0, m_Splines.Length);
		
		m_Disc.Rank   = _Rank;
		m_Path.Phase  = 0;
		m_Path.Spline = m_Splines[index];
		
		m_Flare.Play(m_Duration - 0.1f);
		
		try
		{
			m_SoundProcessor.Play(m_FlySound);
			
			await UnityTask.Phase(
				_Phase => m_Path.Phase = _Phase,
				m_Duration,
				m_Curve
			);
			
			m_SoundProcessor.Play(m_CollectSound);
			m_HapticProcessor.Process(Haptic.Type.ImpactSoft);
			
			await Task.Yield();
		}
		catch (TaskCanceledException)
		{
			return;
		}
		
		m_Path.Phase = 0;
		m_Disc.Rank  = RankType.None;
		
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