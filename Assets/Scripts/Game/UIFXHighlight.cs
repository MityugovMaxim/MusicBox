using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UIFXHighlight : UIEntity
{
	[SerializeField] float          m_Duration;
	[SerializeField] float          m_Source = 1;
	[SerializeField] float          m_Target = 0;
	[SerializeField] CanvasGroup    m_CanvasGroup;
	[SerializeField] RectTransform  m_Background;
	[SerializeField] AnimationCurve m_Curve = AnimationCurve.Linear(0, 0, 1, 1);

	CancellationTokenSource m_TokenSource;

	public async Task PlayAsync()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		try
		{
			await Task.WhenAll(
				AlphaAsync(token),
				WidthAsync(token)
			);
		}
		catch (TaskCanceledException) { }
		
		if (token.IsCancellationRequested)
			return;
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	Task AlphaAsync(CancellationToken _Token)
	{
		m_CanvasGroup.alpha = m_Source;
		
		return UnityTask.Lerp(
			_Value => m_CanvasGroup.alpha = _Value,
			m_Source,
			m_Target,
			m_Duration,
			m_Curve,
			_Token
		);
	}

	Task WidthAsync(CancellationToken _Token)
	{
		const float sourceMin = 0;
		const float sourceMax = 1;
		const float targetMin = 0.45f;
		const float targetMax = 0.55f;
		
		m_Background.anchorMin = new Vector2(sourceMin, m_Background.anchorMin.y);
		m_Background.anchorMax = new Vector2(sourceMax, m_Background.anchorMax.y);
		
		return UnityTask.Phase(
			_Phase =>
			{
				Vector2 anchorMin = m_Background.anchorMin;
				Vector2 anchorMax = m_Background.anchorMax;
				float   phase     = m_Curve.Evaluate(_Phase);
				anchorMin.x            = Mathf.Lerp(sourceMin, targetMin, phase);
				anchorMax.x            = Mathf.Lerp(sourceMax, targetMax, phase);
				m_Background.anchorMin = anchorMin;
				m_Background.anchorMax = anchorMax;
			},
			m_Duration * 0.4f,
			m_Duration * 0.6f,
			_Token
		);
	}
}