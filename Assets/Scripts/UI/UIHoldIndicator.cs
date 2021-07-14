using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UIHoldIndicator : UIIndicator
{
	public override UIHandle Handle      => m_Handle;
	public override float   MinPadding => m_MinCap.rect.yMin;
	public override float   MaxPadding => m_MaxCap.rect.yMax;

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
	[SerializeField] RectTransform    m_MinCap;
	[SerializeField] RectTransform    m_MaxCap;
	[SerializeField] UISpline         m_Spline;
	[SerializeField] UISplineProgress m_Highlight;
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
		
		if (m_MinCap != null)
			m_MinCap.anchoredPosition = m_Spline.First().Position; 
		
		if (m_MaxCap != null)
			m_MaxCap.anchoredPosition = m_Spline.Last().Position;
		
		if (m_Handle != null)
		{
			m_Handle.OnSuccess   += Success;
			m_Handle.OnFail      += Fail;
			m_Handle.OnStartHold += StartHold;
			m_Handle.OnStopHold  += StopHold;
			
			m_Handle.RectTransform.anchoredPosition =  m_Spline.Evaluate(0);
		}
	}

	public void Progress(float _Progress)
	{
		if (m_Spline == null)
			return;
		
		float   phase    = m_Spline.EvaluateVertical(_Progress);
		Vector2 position = m_Spline.Evaluate(phase);
		
		if (m_Highlight != null)
			m_Highlight.Offset = phase;
		
		if (m_Handle != null)
		{
			m_Handle.Progress(_Progress);
			m_Handle.RectTransform.anchoredPosition = position;
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
		Animator.Update(0);
	}

	void Success()
	{
		Animator.SetTrigger(m_SuccessParameterID);
	}

	void Fail()
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
