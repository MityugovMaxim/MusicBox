using System;
using System.Threading.Tasks;
using UnityEngine;

public class UIBannerItem : UIGroup
{
	public enum BannerState
	{
		Open,
		Close
	}

	[SerializeField] UIBannerImage m_Image;
	[SerializeField] UIGroup       m_Control;

	Action<BannerState> m_ProcessFinished;

	string m_BannerID;

	public void Setup(string _BannerID)
	{
		m_BannerID = _BannerID;
		
		m_Image.Setup(m_BannerID);
		
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