using System.Collections;
using System.Linq;
using AudioBox.ASF;
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

	[SerializeField] UIHoldHandle m_Handle;
	[SerializeField] UISpline     m_Spline;
	[SerializeField] UILine       m_Line;
	[SerializeField] UILine       m_Highlight;
	[SerializeField] float        m_SamplesPerUnit = 0.5f;
	[SerializeField] float        m_Weight         = 0.25f;

	ASFHoldClip m_Clip;

	IEnumerator m_HighlightRoutine;

	public void Build(ASFHoldClip _Clip)
	{
		m_Clip = _Clip;
		
		m_Spline.Fill(_Clip.Keys.Select(GetKeyPosition));
		
		m_Spline.Smooth(m_Weight);
		
		m_Spline.Resample(m_SamplesPerUnit);
		
		m_Line.Min = 0;
		m_Line.Max = 1;
		
		m_Highlight.Min = 0;
		m_Highlight.Max = 0;
		
		m_Handle.RectTransform.anchoredPosition = m_Spline.Evaluate(0);
	}

	public override void Restore()
	{
		m_Line.Min = 0;
		m_Line.Max = 1;
		
		m_Highlight.Min = 0;
		m_Highlight.Max = 0;
		
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
		
		m_Line.Min      = phase;
		m_Highlight.Min = phase;
		
		m_Handle.Process(phase);
		m_Handle.RectTransform.anchoredPosition = position;
	}

	public void Success(float _Progress, float _Length)
	{
		Animator.SetTrigger(m_SuccessParameterID);
		Animator.SetBool(m_HoldParameterID, false);
		
		FXProcessor.HoldFX(Handle.GetWorldRect(), _Progress);
		
		HapticProcessor.Process(Haptic.Type.ImpactMedium);
		
		ScoreManager.HoldHit(_Progress, _Length);
		
		InvokeCallback();
	}

	public void Fail()
	{
		Animator.SetTrigger(m_FailParameterID);
		Animator.SetBool(m_HoldParameterID, false);
		
		FXProcessor.Fail();
		
		HapticProcessor.Process(Haptic.Type.ImpactSoft);
		
		ScoreManager.HoldFail();
		
		InvokeCallback();
	}

	public void Hit()
	{
		Highlight();
		
		Animator.SetBool(m_HoldParameterID, true);
		
		HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		InvokeCallback();
	}

	void Highlight()
	{
		if (m_HighlightRoutine != null)
			StopCoroutine(m_HighlightRoutine);
		
		if (!gameObject.activeInHierarchy)
			return;
		
		m_HighlightRoutine = HighlightRoutine();
		
		StartCoroutine(m_HighlightRoutine);
	}

	Vector2 GetKeyPosition(ASFHoldKey _Key)
	{
		Rect rect = ClipRect
			.Transform(Container, RectTransform)
			.HorizontalPadding(Padding);
		
		return new Vector2(
			ASFMath.PhaseToPosition(_Key.Position, rect.xMin, rect.xMax),
			ASFMath.TimeToPosition(_Key.Time, 0, m_Clip.Length, rect.yMin, rect.yMax)
		);
	}

	IEnumerator HighlightRoutine()
	{
		float source = m_Spline.GetLength(m_Highlight.Min);
		float target = m_Spline.GetLength(1);
		
		const float speed = 2000;
		
		float duration = Mathf.Abs(target - source) / speed;
		
		m_Highlight.Max = m_Highlight.Min;
		
		float time = 0;
		while (time < duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			m_Highlight.Max = Mathf.LerpUnclamped(m_Highlight.Min, 1, time / duration);
		}
		
		m_Highlight.Max = target;
	}
}
