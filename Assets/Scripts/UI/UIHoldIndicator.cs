using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIHoldIndicator : UIIndicator
{
	[Preserve]
	public class Pool : MonoMemoryPool<UIHoldIndicator>
	{
		protected override void Reinitialize(UIHoldIndicator _Item)
		{
			_Item.Restore();
		}
	}

	public override UIHandle Handle => m_Handle;

	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_SuccessParameterID = Animator.StringToHash("Success");
	static readonly int m_FailParameterID    = Animator.StringToHash("Fail");
	static readonly int m_HoldParameterID    = Animator.StringToHash("Hold");

	[SerializeField] UIHoldHandle     m_Handle;
	[SerializeField] UISpline         m_Spline;
	[SerializeField] UISplineProgress m_Highlight;
	[SerializeField] UISplineProgress m_Progress;
	[SerializeField] float            m_SamplesPerUnit = 0.5f;

	IEnumerator m_HighlightRoutine;

	public void Setup(HoldClip _Clip, float _Distance)
	{
		if (m_Spline == null)
			return;
		
		Rect rect = RectTransform.rect;
		
		m_Spline.ClearKeys();
		
		Vector2 size = new Vector2(rect.width, _Distance);
		
		foreach (HoldCurve.Key key in _Clip.Curve)
		{
			Vector2 position = new Vector2(
				rect.x + size.x * key.Value,
				size.y * key.Time
			);
			
			position = RectTransform.TransformPoint(position);
			position = m_Spline.RectTransform.InverseTransformPoint(position);
			
			Vector2 inTangent  = Vector2.Scale(size, key.InTangent.Rotate90());
			Vector2 outTangent = Vector2.Scale(size, key.OutTangent.Rotate90());
			
			m_Spline.AddKey(
				new UISpline.Key()
				{
					Position   = position,
					InTangent  = inTangent,
					OutTangent = outTangent,
				}
			);
		}
		
		// Build spline with low amount samples for calculating length of spline
		m_Spline.Samples = 25;
		m_Spline.Rebuild();
		
		// Calculate samples amount by spline length
		m_Spline.Samples = Mathf.CeilToInt(m_Spline.GetLength(1) * m_SamplesPerUnit);
		m_Spline.Rebuild();
		
		if (m_Highlight != null)
		{
			m_Highlight.Min = 0;
			m_Highlight.Max = 0;
		}
		
		if (m_Progress != null)
		{
			m_Progress.Min = 0;
			m_Progress.Max = 0;
		}
		
		if (m_Handle != null)
		{
			m_Handle.Setup(this);
			m_Handle.RectTransform.anchoredPosition = m_Spline.Evaluate(0);
		}
	}

	public void Process(float _Phase)
	{
		if (m_Spline == null)
			return;
		
		float   phase    = m_Spline.EvaluateVertical(_Phase);
		Vector2 position = m_Spline.Evaluate(phase);
		
		if (m_Highlight != null)
			m_Highlight.Min = phase;
		
		if (m_Handle != null)
		{
			m_Handle.Process(phase);
			m_Handle.RectTransform.anchoredPosition = position;
			
			if (m_Progress != null)
			{
				m_Progress.Min = m_Handle.MinProgress;
				m_Progress.Max = m_Handle.MaxProgress;
			}
		}
	}

	public void Restore()
	{
		if (m_Handle != null)
		{
			m_Handle.Process(0);
			m_Handle.RectTransform.anchoredPosition = m_Spline.Evaluate(0);
		}
		
		Animator.ResetTrigger(m_SuccessParameterID);
		Animator.ResetTrigger(m_FailParameterID);
		Animator.SetBool(m_HoldParameterID, false);
		Animator.SetTrigger(m_RestoreParameterID);
		Animator.Update(0);
	}

	[Preserve]
	public void Success(float _MinProgress, float _MaxProgress)
	{
		SignalBus.Fire(new HoldSuccessSignal(_MinProgress, _MaxProgress));
		
		Animator.SetTrigger(m_SuccessParameterID);
		Animator.SetBool(m_HoldParameterID, false);
	}

	[Preserve]
	public void Fail(float _MinProgress, float _MaxProgress)
	{
		SignalBus.Fire(new HoldFailSignal(_MinProgress, _MaxProgress));
		
		Animator.SetTrigger(m_FailParameterID);
		Animator.SetBool(m_HoldParameterID, false);
	}

	[Preserve]
	public void Hit(float _MinProgress, float _MaxProgress)
	{
		SignalBus.Fire(new HoldHitSignal(_MinProgress, _MaxProgress));
		
		Highlight();
		
		Animator.SetBool(m_HoldParameterID, true);
	}

	[Preserve]
	public void Miss(float _MinProgress, float _MaxProgress)
	{
		SignalBus.Fire(new HoldMissSignal(_MinProgress, _MaxProgress));
	}

	void Highlight()
	{
		if (m_HighlightRoutine != null)
			StopCoroutine(m_HighlightRoutine);
		
		m_HighlightRoutine = HighlightRoutine();
		
		StartCoroutine(m_HighlightRoutine);
	}

	IEnumerator HighlightRoutine()
	{
		if (m_Highlight == null)
			yield break;
		
		float source = m_Spline.GetLength(m_Highlight.Min);
		float target = m_Spline.GetLength(1);
		
		const float speed = 1500;
		
		float duration = Mathf.Abs(target - source) / speed;
		
		float time = 0;
		
		m_Highlight.Max = m_Highlight.Min;
		
		while (time < duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			m_Highlight.Max = Mathf.Lerp(m_Highlight.Min, 1, time / duration);
		}
		
		m_Highlight.Max = 1;
	}
}
