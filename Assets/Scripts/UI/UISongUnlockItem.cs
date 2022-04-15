using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UISongUnlockItem : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UISongUnlockItem> { }

	static readonly int m_PlayParameterID    = Animator.StringToHash("Play");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	[SerializeField] UISongImage m_Image;

	[Header("Sounds")]
	[SerializeField, Sound] string m_PlaySound;

	[Inject] SoundProcessor  m_SoundProcessor;
	[Inject] HapticProcessor m_HapticProcessor;

	string m_SongID;

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

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Image.Setup(m_SongID);
		
		Restore();
	}

	public void Play(Action _Finished = null)
	{
		InvokePlayFinished();
		
		m_PlayFinished = _Finished;
		
		m_HapticProcessor.Process(Haptic.Type.ImpactMedium);
		
		m_Animator.SetTrigger(m_PlayParameterID);
	}

	public Task PlayAsync(bool _Instant = false)
	{
		InvokePlayFinished();
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		m_PlayFinished = () => completionSource.SetResult(true);
		
		if (!_Instant && gameObject.activeInHierarchy)
		{
			m_SoundProcessor.Play(m_PlaySound);
			
			m_HapticProcessor.Process(Haptic.Type.ImpactMedium);
			
			m_Animator.SetTrigger(m_PlayParameterID);
		}
		else
		{
			InvokePlayFinished();
		}
		
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