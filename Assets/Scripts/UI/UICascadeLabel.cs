using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
[RequireComponent(typeof(Animator))]
public abstract class UICascadeLabel : UIEntity
{
	static readonly int m_PlayParameterID = Animator.StringToHash("Play");

	public float Length
	{
		get => m_Length;
		set
		{
			if (Mathf.Approximately(m_Length, value))
				return;
			
			m_Length = value;
			
			ProcessLength();
		}
	}

	public float Scale
	{
		get => m_Scale;
		set
		{
			if (Mathf.Approximately(m_Scale, value))
				return;
			
			m_Scale = value;
			
			ProcessScale();
		}
	}

	protected abstract TMP_Text this[int _Index] { get; }

	protected abstract int Count { get; }

	[SerializeField]        float       m_Length;
	[SerializeField]        float       m_Scale;
	[SerializeField]        Vector2     m_Normal;
	[SerializeField]        float       m_MinAngle;
	[SerializeField]        float       m_MaxAngle;
	[SerializeField]        Gradient    m_Gradient;
	[SerializeField]        Haptic.Type m_Haptic;
	[SerializeField, Sound] string      m_Sound;

	[Inject] SoundProcessor  m_SoundProcessor;
	[Inject] HapticProcessor m_HapticProcessor;

	Vector2  m_Offset;
	Animator m_Animator;
	Action   m_PlayFinished;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
		
		StateBehaviour.RegisterComplete(m_Animator, "play", InvokePlayFinished);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessScale();
		
		ProcessLength();
		
		ProcessGradient();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		StateBehaviour.UnregisterComplete(m_Animator, "play", InvokePlayFinished);
	}

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		ProcessLength();
		
		ProcessScale();
		
		#if UNITY_EDITOR
		if (Application.isPlaying)
			return;
		
		if (m_Length > float.Epsilon || m_Scale > float.Epsilon)
			EnableLabels();
		else
			DisableLabels();
		#endif
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessLength();
		
		ProcessScale();
		
		ProcessGradient();
		
		if (m_Length > float.Epsilon || m_Scale > float.Epsilon)
			EnableLabels();
		else
			DisableLabels();
	}
	#endif

	public Task PlayAsync()
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		Play(() => completionSource.SetResult(true));
		
		return completionSource.Task;
	}

	public void Play(Action _Finished = null)
	{
		InvokePlayFinished();
		
		EnableLabels();
		
		m_PlayFinished = () =>
		{
			DisableLabels();
			
			_Finished?.Invoke();
		};
		
		if (gameObject.activeInHierarchy)
		{
			m_Offset = Quaternion.Euler(0, 0, Random.Range(m_MinAngle, m_MaxAngle)) * m_Normal;
			
			m_SoundProcessor.Play(m_Sound);
			
			m_HapticProcessor.Process(m_Haptic);
			
			m_Animator.SetTrigger(m_PlayParameterID);
		}
		else
		{
			DisableLabels();
			
			InvokePlayFinished();
		}
	}

	void EnableLabels()
	{
		for (int i = 0; i < Count; i++)
			this[i].gameObject.SetActive(true);
	}

	void DisableLabels()
	{
		for (int i = 0; i < Count - 1; i++)
			this[i].gameObject.SetActive(false);
	}

	void ProcessLength()
	{
		float step = 1.0f / (Count - 1);
		for (int i = 0; i < Count; i++)
			this[i].rectTransform.anchoredPosition = m_Offset * m_Length * step * i;
	}

	void ProcessScale()
	{
		float step = 1.0f / (Count - 1);
		for (int i = 0; i < Count; i++)
			this[i].rectTransform.localScale = Vector3.one * (1 + m_Scale * step * i);
	}

	void ProcessGradient()
	{
		float step = 1.0f / (Count - 1);
		for (int i = 0; i < Count; i++)
			this[i].color = m_Gradient.Evaluate(step * i);
	}

	void InvokePlayFinished()
	{
		Action action = m_PlayFinished;
		m_PlayFinished = null;
		action?.Invoke();
	}
}
