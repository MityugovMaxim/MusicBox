using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UIPulseSpectrum : UISpectrum
{
	[SerializeField, Range(0, 1)] float m_Threshold = 0.5f;
	[SerializeField]              float m_Duration  = 0.2f;
	[SerializeField]              float m_Delay     = 0.2f;
	[SerializeField]              float m_MinScale  = 1;
	[SerializeField]              float m_MaxScale  = 1.5f;
	[SerializeField]              int   m_Channel;

	CancellationTokenSource m_TokenSource;

	bool  m_Active;
	float m_Time;

	public override void Reposition() { }

	public override void Sample(float[] _Amplitude)
	{
		if (Time.time < m_Time)
			return;
		
		int channel = Mathf.Clamp(m_Channel, 0, _Amplitude.Length - 1);
		
		float amplitude = _Amplitude[channel];
		
		if (amplitude < m_Threshold)
		{
			m_Active = false;
			return;
		}
		
		if (m_Active)
			return;
		
		m_Time = Time.time + m_Delay;
		
		Pulse();
	}

	async void Pulse()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		try
		{
			await UnityTask.Phase(
				_Phase =>
				{
					float scale = Mathf.Lerp(m_MaxScale, m_MinScale, _Phase);
					RectTransform.localScale = new Vector3(scale, scale, 1);
				},
				m_Duration,
				EaseFunction.EaseInQuad,
				token
			);
		}
		catch (TaskCanceledException) { }
		
		if (token.IsCancellationRequested)
			return;
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}
}