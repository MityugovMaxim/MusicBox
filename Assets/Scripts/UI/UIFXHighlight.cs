using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIFXHighlight : UIEntity
{
	CanvasGroup CanvasGroup
	{
		get
		{
			if (m_CanvasGroup == null)
				m_CanvasGroup = GetComponent<CanvasGroup>();
			return m_CanvasGroup;
		}
	}

	[SerializeField] float          m_Duration;
	[SerializeField] float          m_Source = 1;
	[SerializeField] float          m_Target = 0;
	[SerializeField] AnimationCurve m_Curve  = AnimationCurve.Linear(0, 0, 1, 1);

	CanvasGroup m_CanvasGroup;

	CancellationTokenSource m_TokenSource;

	public async Task PlayAsync()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		CanvasGroup canvasGroup = CanvasGroup;
		
		try
		{
			
			await UnityTask.Phase(
				_Phase => canvasGroup.alpha = Mathf.Lerp(m_Source, m_Target, m_Curve.Evaluate(_Phase)),
				m_Duration,
				token
			);
		}
		catch (TaskCanceledException) { }
		
		canvasGroup.alpha = 0;
		
		if (token.IsCancellationRequested)
			return;
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}
}