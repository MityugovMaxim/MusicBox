using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UIHoldIndicator : UIIndicator
{
	public override UIHandle Handle      => m_Handle;
	public override float   MinPadding => 150;
	public override float   MaxPadding => 150;

	Animator Animator
	{
		get
		{
			if (m_Animator == null)
				m_Animator = GetComponent<Animator>();
			return m_Animator;
		}
	}

	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_SuccessParameterID = Animator.StringToHash("Success");
	static readonly int m_FailParameterID    = Animator.StringToHash("Fail");
	static readonly int m_HoldParameterID    = Animator.StringToHash("Hold");

	[SerializeField] UIHoldHandle     m_Handle;
	[SerializeField] UISpline         m_Spline;
	[SerializeField] UISplineProgress m_Highlight;
	[SerializeField] UISplineProgress m_Progress;
	[SerializeField] float            m_SamplesPerUnit = 0.5f;

	Animator m_Animator;

	public void Setup(HoldClip _Clip, float _Distance)
	{
		if (m_Spline == null)
			return;
		
		Rect rect = RectTransform.rect;
		
		m_Spline.ClearKeys();
		
		m_Spline.Samples = 25;
		
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
		
		m_Spline.Samples = Mathf.FloorToInt(m_Spline.GetLength(1) * m_SamplesPerUnit);
		
		m_Spline.Rebuild();
		
		if (m_Progress != null)
		{
			m_Progress.Min = 0;
			m_Progress.Max = 0;
		}
		
		if (m_Handle != null)
		{
			m_Handle.OnSuccess   += Success;
			m_Handle.OnFail      += Fail;
			m_Handle.OnStartHold += StartHold;
			m_Handle.OnStopHold  += StopHold;
			
			m_Handle.RectTransform.anchoredPosition =  m_Spline.Evaluate(0);
		}
	}

	public void Process(float _Phase)
	{
		if (m_Spline == null)
			return;
		
		float   phase    = m_Spline.EvaluateVertical(_Phase);
		Vector2 position = m_Spline.Evaluate(phase);
		
		if (m_Highlight != null)
			m_Highlight.Offset = phase;
		
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
			m_Handle.OnSuccess   -= Success;
			m_Handle.OnFail      -= Fail;
			m_Handle.OnStartHold -= StartHold;
			m_Handle.OnStopHold  -= StopHold;
		}
		
		Animator.ResetTrigger(m_SuccessParameterID);
		Animator.ResetTrigger(m_FailParameterID);
		Animator.SetBool(m_HoldParameterID, false);
		Animator.SetTrigger(m_RestoreParameterID);
		
		if (gameObject.activeInHierarchy)
			Animator.Update(0);
	}

	void Success(float _Progress)
	{
		Animator.SetTrigger(m_SuccessParameterID);
	}

	void Fail(float _Progress)
	{
		Animator.SetTrigger(m_FailParameterID);
	}

	void StartHold()
	{
		Animator.SetBool(m_HoldParameterID, true);
	}

	void StopHold()
	{
		Animator.SetBool(m_HoldParameterID, false);
	}
}
