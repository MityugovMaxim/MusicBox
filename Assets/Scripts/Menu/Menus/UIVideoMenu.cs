using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

[Menu(MenuType.VideoMenu)]
public class UIVideoMenu : UIMenu
{
	[SerializeField] VideoPlayer m_Player;

	bool m_Processed;

	Action m_Finished;

	public void Setup(VideoClip _Clip)
	{
		m_Player.clip = _Clip;
	}

	public Task ProcessAsync()
	{
		if (m_Processed)
			return Task.CompletedTask;
		
		InvokeFinished();
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		m_Finished = () => completionSource.TrySetResult(true);
		
		return completionSource.Task;
	}

	public void Close()
	{
		Hide();
	}

	protected override void OnShowStarted()
	{
		m_Processed = false;
	}

	protected override async void OnShowFinished()
	{
		m_Player.Stop();
		
		await PrepareAsync();
		
		m_Player.Play();
	}

	protected override void OnHideFinished()
	{
		m_Processed = true;
		
		m_Player.Stop();
		
		InvokeFinished();
	}

	Task PrepareAsync()
	{
		if (m_Player.isPrepared)
			return Task.CompletedTask;
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		void Complete(VideoPlayer _Player)
		{
			m_Player.prepareCompleted -= Complete;
			
			completionSource.TrySetResult(true);
		}
		
		m_Player.prepareCompleted += Complete;
		
		m_Player.Prepare();
		
		return completionSource.Task;
	}

	void InvokeFinished()
	{
		Action action = m_Finished;
		m_Finished = null;
		action?.Invoke();
	}
}