using System;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UIFlare : UIGroup
{
	[SerializeField] Graphic[]      m_Flares;
	[SerializeField] float          m_MinSize     = 10;
	[SerializeField] float          m_MaxSize     = 20;
	[SerializeField] float          m_MinDelay    = 0.1f;
	[SerializeField] float          m_MaxDelay    = 0.15f;
	[SerializeField] float          m_MinDuration = 0.1f;
	[SerializeField] float          m_MaxDuration = 0.15f;
	[SerializeField] AnimationCurve m_Curve       = AnimationCurve.EaseInOut(0, 0, 1, 1);

	CancellationTokenSource m_TokenSource;

	protected override void OnShowStarted()
	{
		Stop();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		float delay = 0;
		
		foreach (Graphic flare in m_Flares)
		{
			Play(flare, delay, token);
			
			delay += Random.Range(m_MinDelay, m_MaxDelay);
		}
	}

	protected override void OnHideFinished()
	{
		Stop();
	}

	async void Play(Graphic _Flare, float _Delay, CancellationToken _Token = default)
	{
		CancellationToken token = m_TokenSource.Token;
		
		Color color       = _Flare.color;
		Color sourceColor = new Color(color.r, color.g, color.b, 1);
		Color targetColor = new Color(color.r, color.g, color.b, 0);
		
		RectTransform transform = _Flare.rectTransform;
		
		while (true)
		{
			if (token.IsCancellationRequested)
				return;
			
			float duration   = Random.Range(m_MinDuration, m_MaxDuration);
			float size       = Random.Range(m_MinSize, m_MaxSize);
			
			Vector2 sourceSize = new Vector2(size, size);
			Vector2 targetSize = new Vector2(0, 0);
			
			_Flare.color        = targetColor;
			transform.sizeDelta = sourceSize;
			
			try
			{
				await UnityTask.Phase(
					_Phase =>
					{
						_Flare.color        = Color.Lerp(sourceColor, targetColor, _Phase);
						transform.sizeDelta = Vector2.Lerp(sourceSize, targetSize, _Phase);
					},
					_Delay,
					duration,
					m_Curve,
					token
				);
			}
			catch (TaskCanceledException) { }
			catch (Exception exception) { Log.Exception(this, exception); }
		}
	}

	void Stop()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}
}