using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIResultDisc : UIEntity
{
	const string CLAIM_STATE   = "claim";
	const string COLLECT_STATE = "collect";

	static readonly int m_ActiveParameterID  = Animator.StringToHash("Active");
	static readonly int m_ClaimParameterID   = Animator.StringToHash("Claim");
	static readonly int m_CollectParameterID = Animator.StringToHash("Collect");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	[SerializeField] UIFlare     m_Flare;
	[SerializeField] Haptic.Type m_Haptic;

	[SerializeField, Sound] string m_Sound;

	[Inject] HapticProcessor m_HapticProcessor;
	[Inject] SoundProcessor  m_SoundProcessor;

	Animator m_Animator;

	Action m_ActivateFinished;
	Action m_ClaimFinished;
	Action m_CollectFinished;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
		
		m_Animator.SubscribeComplete(CLAIM_STATE, InvokeClaimFinished);
		m_Animator.SubscribeComplete(COLLECT_STATE, InvokeCollectFinished);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		if (m_Animator == null)
			return;
		
		m_Animator.UnsubscribeComplete(CLAIM_STATE, InvokeClaimFinished);
		m_Animator.UnsubscribeComplete(COLLECT_STATE, InvokeCollectFinished);
	}

	public void Activate()
	{
		m_Animator.SetBool(m_ActiveParameterID, true);
	}

	public void Deactivate()
	{
		m_Animator.SetBool(m_ActiveParameterID, false);
	}

	public async void Claim() => await ClaimAsync();

	public Task ClaimAsync()
	{
		InvokeClaimFinished();
		
		TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
		
		m_ClaimFinished = () => task.TrySetResult(true);
		
		m_Animator.SetTrigger(m_ClaimParameterID);
		
		m_Flare.Play();
		
		return task.Task;
	}

	public Task CollectAsync()
	{
		InvokeCollectFinished();
		
		TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
		
		m_CollectFinished = () => task.TrySetResult(true);
		
		m_Animator.SetTrigger(m_CollectParameterID);
		
		return task.Task;
	}

	public void Restore()
	{
		m_Animator.SetBool(m_ActiveParameterID, false);
		m_Animator.ResetTrigger(m_CollectParameterID);
		m_Animator.SetTrigger(m_RestoreParameterID);
		m_Animator.Update(0);
	}

	[UsedImplicitly]
	void PlayHaptic() => m_HapticProcessor.Process(m_Haptic);

	[UsedImplicitly]
	void PlaySound() => m_SoundProcessor.Play(m_Sound);

	[UsedImplicitly]
	void PlayFlare() => m_Flare.Play();

	void InvokeClaimFinished()
	{
		Action action = m_ClaimFinished;
		m_ClaimFinished = null;
		action?.Invoke();
	}

	void InvokeCollectFinished()
	{
		Action action = m_CollectFinished;
		m_CollectFinished = null;
		action?.Invoke();
	}
}
