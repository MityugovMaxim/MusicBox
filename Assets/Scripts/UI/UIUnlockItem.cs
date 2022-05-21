using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIUnlockItem : UIEntity
{
	public enum UnlockType
	{
		Song  = 0,
		Coins = 1,
	}

	[Preserve]
	public class Pool : UIEntityPool<UIUnlockItem> { }

	const string PLAY_STATE = "play";

	static readonly int m_PlayParameterID    = Animator.StringToHash("Play");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	[SerializeField] WebGraphic  m_Image;
	[SerializeField] TMP_Text    m_Label;
	[SerializeField] UIUnitLabel m_Value;

	[SerializeField, Sound] string m_Sound;

	[Inject] SoundProcessor  m_SoundProcessor;
	[Inject] HapticProcessor m_HapticProcessor;

	Animator m_Animator;
	Action   m_PlayFinished;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
		
		m_Animator.RegisterComplete(PLAY_STATE, InvokePlayFinished);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Animator.UnregisterComplete(PLAY_STATE, InvokePlayFinished);
	}

	public void Setup(string _Path, string _Label = null, long _Value = 0)
	{
		m_Image.Path  = _Path;
		m_Label.text  = _Label;
		m_Value.Value = _Value;
		
		m_Label.gameObject.SetActive(!string.IsNullOrEmpty(_Label));
		m_Value.gameObject.SetActive(_Value != 0);
		
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
			m_SoundProcessor.Play(m_Sound);
			
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
	}

	void InvokePlayFinished()
	{
		Action action = m_PlayFinished;
		m_PlayFinished = null;
		action?.Invoke();
	}
}