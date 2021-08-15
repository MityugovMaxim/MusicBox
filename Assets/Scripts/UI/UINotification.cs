using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class UINotification : UIEntity, IPointerClickHandler, IEndDragHandler
{
	static readonly int m_ShowParameterID = Animator.StringToHash("Show");
	static readonly int m_HideParameterID = Animator.StringToHash("Hide");

	[SerializeField] Image    m_Image;
	[SerializeField] TMP_Text m_Title;
	[SerializeField] TMP_Text m_Description;

	Animator       m_Animator;
	StateBehaviour m_HideState;
	Action         m_Action;
	Action         m_Finished;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_HideState = StateBehaviour.GetBehaviour(m_Animator, "hide");
		if (m_HideState != null)
			m_HideState.OnComplete += InvokeFinished;
	}

	public void Setup(
		Sprite _Sprite,
		string _Title,
		string _Description,
		Action _Action
	)
	{
		m_Image.sprite     = _Sprite;
		m_Title.text       = _Title;
		m_Description.text = _Description;
		m_Action           = _Action;
	}

	public void Play(Action _Finished)
	{
		InvokeFinished();
		
		m_Finished = _Finished;
		
		m_Animator.ResetTrigger(m_HideParameterID);
		m_Animator.SetTrigger(m_ShowParameterID);
	}

	void InvokeFinished()
	{
		Action action = m_Finished;
		m_Finished = null;
		action?.Invoke();
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		Action action = m_Action;
		m_Action = null;
		action?.Invoke();
		
		m_Animator.ResetTrigger(m_ShowParameterID);
		m_Animator.SetTrigger(m_HideParameterID);
	}

	public void OnEndDrag(PointerEventData _EventData)
	{
		if (_EventData.delta.y > 0)
			return;
		
		m_Action = null;
		
		m_Animator.ResetTrigger(m_ShowParameterID);
		m_Animator.SetTrigger(m_HideParameterID);
	}
}
