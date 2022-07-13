using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UITutorialFingers : UIOrder
{
	public enum Gesture
	{
		Tap    = 0,
		Double = 1,
		Hold   = 2,
		Bend   = 3,
	}

	public override int Thickness => 1;

	static readonly int m_ShowParameterID = Animator.StringToHash("Show");
	static readonly int m_TypeParameterID = Animator.StringToHash("Type");

	Animator m_Animator;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
	}

	public void Setup(float _Ratio)
	{
		float position = 1.0f - _Ratio;
		
		Vector2 anchorMin = RectTransform.anchorMin;
		Vector2 anchorMax = RectTransform.anchorMax;
		
		anchorMin.y = position;
		anchorMax.y = position;
		
		RectTransform.anchorMin = anchorMin;
		RectTransform.anchorMax = anchorMax;
	}

	public void Show(Gesture _Gesture)
	{
		m_Animator.SetInteger(m_TypeParameterID, (int)_Gesture);
		m_Animator.SetBool(m_ShowParameterID, true);
	}

	public void Hide()
	{
		m_Animator.SetBool(m_ShowParameterID, false);
	}
}