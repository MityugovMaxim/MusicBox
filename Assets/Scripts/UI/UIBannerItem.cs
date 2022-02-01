using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIBannerItem : UIGroup
{
	public enum BannerState
	{
		Open,
		Close
	}

	[SerializeField] UIRemoteImage m_Banner;
	[SerializeField] UIGroup       m_Control;

	StorageProcessor m_StorageProcessor;

	Action<BannerState> m_ProcessFinished;

	[Inject]
	public void Construct(StorageProcessor _StorageProcessor)
	{
		m_StorageProcessor = _StorageProcessor;
	}

	public void Setup(string _BannerID)
	{
		m_Banner.Load(m_StorageProcessor.LoadLevelThumbnail(_BannerID));
		
		m_Control.Hide(true);
	}

	public Task<BannerState> Process(bool _Permanent)
	{
		InvokeProcessFinished(BannerState.Close);
		
		TaskCompletionSource<BannerState> completionSource = new TaskCompletionSource<BannerState>();
		
		m_ProcessFinished = _State => completionSource.TrySetResult(_State);
		
		if (_Permanent)
			m_Control.Hide(true);
		else
			m_Control.Show();
		
		return completionSource.Task;
	}

	public void Open()
	{
		Debug.LogError("---> OPEN");
		
		InvokeProcessFinished(BannerState.Open);
	}

	public void Close()
	{
		InvokeProcessFinished(BannerState.Close);
	}

	void InvokeProcessFinished(BannerState _State)
	{
		Action<BannerState> action = m_ProcessFinished;
		m_ProcessFinished = null;
		action?.Invoke(_State);
	}
}