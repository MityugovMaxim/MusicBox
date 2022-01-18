using System;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
[RequireComponent(typeof(Animator))]
public class UIMultiplierLabel : UIEntity
{
	Animator Animator
	{
		get
		{
			if (m_Animator == null)
			{
				m_Animator                                      = GetComponent<Animator>();
				m_Animator.keepAnimatorControllerStateOnDisable = true;
			}
			return m_Animator;
		}
	}

	static readonly int m_PlayParameterID = Animator.StringToHash("Play");

	[SerializeField] UIUnitLabel[] m_Labels;
	[SerializeField] Vector2       m_Offset;
	[SerializeField] float         m_Length;
	[SerializeField] float         m_Scale;

	Animator m_Animator;
	Action   m_PlayFinished;

	protected override void Awake()
	{
		base.Awake();
		
		StateBehaviour.RegisterComplete(Animator, "play", InvokePlayFinished);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		StateBehaviour.UnregisterComplete(Animator, "play", InvokePlayFinished);
	}

	public Task PlayAsync(int _Multiplier, bool _Instant = false)
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		Play(_Multiplier, _Instant, () => completionSource.SetResult(true));
		
		return completionSource.Task;
	}

	public void Play(int _Multiplier, bool _Instant = false, Action _Finished = null)
	{
		InvokePlayFinished();
		
		m_PlayFinished = _Finished;
		
		foreach (UIUnitLabel label in m_Labels)
			label.Value = _Multiplier;
		
		if (!_Instant && gameObject.activeInHierarchy)
		{
			m_Offset = Random.rotation * Vector2.up;
			Animator.SetTrigger(m_PlayParameterID);
		}
		else
		{
			InvokePlayFinished();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessLength();
		ProcessScale();
	}

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		ProcessLength();
		ProcessScale();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessLength();
		ProcessScale();
	}
	#endif

	void ProcessLength()
	{
		float step = 1.0f / (m_Labels.Length - 1);
		for (int i = 0; i < m_Labels.Length; i++)
			m_Labels[i].RectTransform.anchoredPosition = m_Offset * m_Length * step * i;
	}

	void ProcessScale()
	{
		float step = 1.0f / (m_Labels.Length - 1);
		for (int i = 0; i < m_Labels.Length; i++)
			m_Labels[i].RectTransform.localScale = Vector3.one * (1 + m_Scale * step * i);
	}

	void InvokePlayFinished()
	{
		Action action = m_PlayFinished;
		m_PlayFinished = null;
		action?.Invoke();
	}
}