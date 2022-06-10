using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UICountdown : UIEntity
{
	static readonly int m_PlayParameterID = Animator.StringToHash("Play");

	[Inject] SoundProcessor m_SoundProcessor;

	Animator m_Animator;

	Action m_Finished;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.RegisterComplete("play", InvokePlayFinished);
	}

	public async void Play()
	{
		await PlayAsync();
	}

	public Task PlayAsync()
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		InvokePlayFinished();
		
		m_Finished = () => completionSource.SetResult(true);
		
		m_Animator.SetTrigger(m_PlayParameterID);
		
		return completionSource.Task;
	}

	[UsedImplicitly]
	void PlaySound(string _SoundID)
	{
		m_SoundProcessor.Play(_SoundID);
	}

	void InvokePlayFinished()
	{
		Action action = m_Finished;
		m_Finished = null;
		action?.Invoke();
	}
}