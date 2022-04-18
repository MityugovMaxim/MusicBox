using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Animator))]
public class 
	UIHealthHandle : UIEntity
{
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_DamageParameterID  = Animator.StringToHash("Damage");

	[SerializeField, Sound] string m_DamageSound;

	[Inject] SoundProcessor m_SoundProcessor;

	Animator m_Animator;
	Action   m_DamageFinished;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
		
		m_Animator.RegisterComplete("damage", InvokeDamageFinished);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Animator.UnregisterComplete("damage", InvokeDamageFinished);
	}

	public void Restore()
	{
		InvokeDamageFinished();
		
		m_Animator.SetTrigger(m_RestoreParameterID);
	}

	public async void Damage()
	{
		await DamageAsync();
	}

	public Task DamageAsync(CancellationToken _Token = default)
	{
		Restore();
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		m_DamageFinished = () => completionSource.TrySetResult(true);
		
		if (_Token.IsCancellationRequested)
		{
			InvokeDamageFinished();
			return completionSource.Task;
		}
		
		_Token.Register(InvokeDamageFinished);
		
		m_SoundProcessor.Play(m_DamageSound);
		
		m_Animator.SetTrigger(m_DamageParameterID);
		
		return completionSource.Task;
	}

	void InvokeDamageFinished()
	{
		Action action = m_DamageFinished;
		m_DamageFinished = null;
		action?.Invoke();
	}
}