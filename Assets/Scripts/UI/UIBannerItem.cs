using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIBannerItem : UIGroup
{
	public enum BannerState
	{
		None,
		Open,
		Close
	}

	[SerializeField] UIBannerImage m_Image;
	[SerializeField] GameObject    m_Control;

	[Inject] BannersProcessor m_BannersProcessor;

	Action<BannerState> m_ProcessFinished;

	string m_BannerID;
	bool   m_Permanent;

	public void Setup(string _BannerID)
	{
		m_BannerID  = _BannerID;
		m_Permanent = m_BannersProcessor.IsPermanent(m_BannerID);
		
		m_Image.Setup(m_BannerID);
		
		m_Control.SetActive(!m_Permanent);
	}

	public Task<BannerState> Process()
	{
		InvokeProcessFinished(BannerState.Close);
		
		TaskCompletionSource<BannerState> completionSource = new TaskCompletionSource<BannerState>();
		
		m_ProcessFinished = _State => completionSource.TrySetResult(_State);
		
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
		action?.Invoke(m_Permanent ? BannerState.None : _State);
	}
}