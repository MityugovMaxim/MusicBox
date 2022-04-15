using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator))]
public class UIDoubleFX : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UIDoubleFX> { }

	static readonly int m_PlayParameterID    = Animator.StringToHash("Play");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	[SerializeField] RectTransform[] m_Glows;
	[SerializeField] float           m_MinGlow;
	[SerializeField] float           m_MaxGlow;

	Animator m_Animator;
	Action   m_PlayFinished;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
		
		m_Animator.RegisterComplete("play", InvokePlayFinished);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Animator.UnregisterComplete("play", InvokePlayFinished);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		Restore();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		Restore();
	}

	public Task PlayAsync(CancellationToken _Token = default)
	{
		Restore();
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		m_PlayFinished = () => completionSource.TrySetResult(true);
		
		if (_Token.IsCancellationRequested)
		{
			InvokePlayFinished();
			return completionSource.Task;
		}
		
		_Token.Register(Restore);
		
		foreach (RectTransform glow in m_Glows)
			glow.sizeDelta = Vector2.one * Random.Range(m_MinGlow, m_MaxGlow);
		
		m_Animator.SetTrigger(m_PlayParameterID);
		
		return completionSource.Task;
	}

	void Restore()
	{
		InvokePlayFinished();
		
		m_Animator.SetTrigger(m_RestoreParameterID);
	}

	void InvokePlayFinished()
	{
		Action action = m_PlayFinished;
		m_PlayFinished = null;
		action?.Invoke();
	}
}