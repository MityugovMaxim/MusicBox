using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIUnlockItem : UIEntity
{
	[Preserve]
	public class Pool : MonoMemoryPool<UIUnlockItem> { }

	static readonly int m_PlayParameterID    = Animator.StringToHash("Play");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	[SerializeField] UIRemoteImage m_Image;

	HapticProcessor m_HapticProcessor;

	Animator m_Animator;
	Action   m_PlayFinished;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
		
		StateBehaviour.RegisterComplete(m_Animator, "play", InvokePlayFinished);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		StateBehaviour.UnregisterComplete(m_Animator, "play", InvokePlayFinished);
	}

	[Inject]
	public void Construct(HapticProcessor _HapticProcessor)
	{
		m_HapticProcessor = _HapticProcessor;
	}

	public void Setup(Task<Sprite> _Sprite)
	{
		m_Image.Load(_Sprite);
		
		Restore();
	}

	public void Play(Action _Finished = null)
	{
		InvokePlayFinished();
		
		m_PlayFinished = _Finished;
		
		m_HapticProcessor.Process(Haptic.Type.ImpactMedium);
		
		m_Animator.SetTrigger(m_PlayParameterID);
	}

	public Task PlayAsync()
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		Play(() => completionSource.SetResult(true));
		
		return completionSource.Task;
	}

	void Restore()
	{
		InvokePlayFinished();
		
		m_Animator.ResetTrigger(m_PlayParameterID);
		m_Animator.SetTrigger(m_RestoreParameterID);
		m_Animator.Update(0);
	}

	void InvokePlayFinished()
	{
		Action action = m_PlayFinished;
		m_PlayFinished = null;
		action?.Invoke();
	}
}