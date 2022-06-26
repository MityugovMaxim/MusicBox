using System;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.ASF;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.Scripting;

public class UIHoldIndicator : UIIndicator
{
	[Preserve]
	public class Pool : UIIndicatorPool<UIHoldIndicator>
	{
		protected override void OnSpawned(UIHoldIndicator _Item)
		{
			base.OnSpawned(_Item);
			
			_Item.Restore();
		}

		protected override void OnDespawned(UIHoldIndicator _Item)
		{
			_Item.Restore();
			
			base.OnDespawned(_Item);
		}
	}

	const int COUNT = 4;

	public override UIHandle Handle => m_Handle;

	float Padding => GetLocalRect().width / (COUNT * 2);

	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_SuccessParameterID = Animator.StringToHash("Success");
	static readonly int m_FailParameterID    = Animator.StringToHash("Fail");
	static readonly int m_HoldParameterID    = Animator.StringToHash("Hold");

	[SerializeField] UIHoldHandle     m_Handle;
	[SerializeField] UISpline         m_Spline;
	[SerializeField] UISplineProgress m_Highlight;
	[SerializeField] UISplineProgress m_Progress;
	[SerializeField] float            m_SamplesPerUnit = 0.5f;
	[SerializeField] float            m_Weight         = 0.25f;

	CancellationTokenSource m_HighlightToken;

	public void Build(ASFHoldClip _Clip)
	{
		m_Spline.ClearKeys();
		
		double length = _Clip.Length;
		foreach (ASFHoldClip.Key key in _Clip.Keys)
		{
			Vector2      position = GetKeyPosition(key.Time, key.Position, length);
			UISpline.Key spline   = new UISpline.Key();
			spline.Position   = position;
			spline.InTangent  = Vector2.zero;
			spline.OutTangent = Vector2.zero;
			
			m_Spline.AddKey(spline);
		}
		
		int count = m_Spline.GetKeysCount();
		for (int i = 1; i < count; i++)
		{
			UISpline.Key source = m_Spline.GetKey(i - 1);
			UISpline.Key target = m_Spline.GetKey(i);
			
			float tangent = Mathf.Abs(target.Position.y - source.Position.y) * m_Weight;
			
			source.OutTangent = new Vector2(0, tangent);
			target.InTangent  = new Vector2(0, -tangent);
			
			m_Spline.SetKey(i - 1, source);
			m_Spline.SetKey(i, target);
		}
		
		// Build spline with low amount samples for calculating length of spline
		m_Spline.Samples = 25;
		m_Spline.Rebuild();
		
		// Calculate samples amount by spline length
		m_Spline.Samples = Mathf.CeilToInt(m_Spline.GetLength(1) * m_SamplesPerUnit);
		m_Spline.Rebuild();
		
		m_Highlight.Min = 0;
		m_Highlight.Max = 0;
		
		m_Progress.Min = 0;
		m_Progress.Max = 0;
		
		m_Handle.RectTransform.anchoredPosition = m_Spline.Evaluate(0);
	}

	public override void Restore()
	{
		m_Highlight.Min = 0;
		m_Highlight.Max = 0;
		
		m_Progress.Min = 0;
		m_Progress.Max = 0;
		
		m_Handle.Restore();
		m_Handle.RectTransform.anchoredPosition = m_Spline.Evaluate(0);
		
		Animator.ResetTrigger(m_SuccessParameterID);
		Animator.ResetTrigger(m_FailParameterID);
		Animator.SetBool(m_HoldParameterID, false);
		Animator.SetTrigger(m_RestoreParameterID);
	}

	public void Process(float _Phase)
	{
		if (m_Spline == null)
			return;
		
		float   phase    = m_Spline.EvaluateVertical(_Phase);
		Vector2 position = m_Spline.Evaluate(phase);
		
		m_Highlight.Min = phase;
		
		m_Handle.Process(phase);
		m_Handle.RectTransform.anchoredPosition = position;
		
		m_Progress.Min = m_Handle.MinProgress;
		m_Progress.Max = m_Handle.MaxProgress;
	}

	public void Success(float _MinProgress, float _MaxProgress)
	{
		Animator.SetTrigger(m_SuccessParameterID);
		Animator.SetBool(m_HoldParameterID, false);
		
		float progress = Mathf.Max(0, _MaxProgress - _MinProgress);
		
		FXProcessor.HoldFX(Handle.GetWorldRect(), progress);
		
		InvokeCallback();
		
		SignalBus.Fire(new HoldSuccessSignal(_MinProgress, _MaxProgress));
	}

	public void Fail(float _MinProgress, float _MaxProgress)
	{
		Animator.SetTrigger(m_FailParameterID);
		Animator.SetBool(m_HoldParameterID, false);
		
		FXProcessor.Fail();
		
		InvokeCallback();
		
		SignalBus.Fire(new HoldFailSignal(_MinProgress, _MaxProgress));
	}

	public void Hit(float _Progress)
	{
		Highlight();
		
		Animator.SetBool(m_HoldParameterID, true);
		
		InvokeCallback();
		
		SignalBus.Fire(new HoldHitSignal(_Progress));
	}

	public void Miss(float _MinProgress, float _MaxProgress)
	{
		SignalBus.Fire(new HoldMissSignal(_MinProgress, _MaxProgress));
	}

	async void Highlight()
	{
		m_HighlightToken?.Cancel();
		m_HighlightToken?.Dispose();
		
		m_HighlightToken = new CancellationTokenSource();
		
		CancellationToken token = m_HighlightToken.Token;
		
		try
		{
			await HighlightAsync(token);
		}
		catch (TaskCanceledException) { }
		catch (Exception exception)
		{
			Log.Exception(this, exception);
		}
		
		m_HighlightToken?.Dispose();
		m_HighlightToken = null;
	}

	Vector2 GetKeyPosition(double _Time, float _Position, double _Length)
	{
		Rect rect = ClipRect
			.Transform(Container, RectTransform)
			.HorizontalPadding(Padding);
		
		return new Vector2(
			ASFMath.PhaseToPosition(_Position, rect.xMin, rect.xMax),
			ASFMath.TimeToPosition(_Time, 0, _Length, rect.yMin, rect.yMax)
		);
	}

	Task HighlightAsync(CancellationToken _Token = default)
	{
		float source = m_Spline.GetLength(m_Highlight.Min);
		float target = m_Spline.GetLength(1);
		
		const float speed = 1500;
		
		float duration = Mathf.Abs(target - source) / speed;
		
		m_Highlight.Max = m_Highlight.Min;
		
		return UnityTask.Phase(
			_Phase => m_Highlight.Max = Mathf.Lerp(m_Highlight.Min, 1, _Phase),
			duration,
			_Token
		);
	}
}
