using System;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UIChestReward : UIEntity
{
	const string PLAY_STATE = "play";

	static readonly int m_PlayParameterID    = Animator.StringToHash("Play");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	[SerializeField] UIChestImage  m_Image;
	[SerializeField] GameObject    m_CoinsContent;
	[SerializeField] GameObject    m_SongContent;
	[SerializeField] GameObject    m_VoucherContent;
	[SerializeField] UICoinsItem   m_CoinsItem;
	[SerializeField] UISongItem    m_SongItem;
	[SerializeField] UIVoucherItem m_VoucherItem;

	Animator m_Animator;

	Action m_Finished;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.SubscribeComplete(PLAY_STATE, InvokeFinished);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Animator.UnsubscribeComplete(PLAY_STATE, InvokeFinished);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		Restore();
	}

	public void Setup(string _ChestID, ChestReward _Reward)
	{
		m_Image.ChestID = _ChestID;
		
		m_CoinsContent.SetActive(false);
		m_SongContent.SetActive(false);
		m_VoucherContent.SetActive(false);
		
		if (_Reward.IsCoins)
		{
			m_CoinsContent.SetActive(true);
			m_CoinsItem.Setup(_Reward.Value);
		}
		else if (_Reward.IsSong)
		{
			m_SongContent.SetActive(true);
			m_SongItem.Setup(_Reward.ID);
		}
		else if (_Reward.IsVoucher)
		{
			m_VoucherContent.SetActive(true);
			m_VoucherItem.Setup(_Reward.ID);
		}
	}

	public async void Play() => await PlayAsync();

	public Task PlayAsync()
	{
		InvokeFinished();
		
		TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
		
		m_Finished = () => source.TrySetResult(true);
		
		m_Animator.SetTrigger(m_PlayParameterID);
		
		return source.Task;
	}

	public void Restore()
	{
		InvokeFinished();
		
		m_Animator.ResetTrigger(m_PlayParameterID);
		m_Animator.SetTrigger(m_RestoreParameterID);
		m_Animator.Update(0);
	}

	void InvokeFinished()
	{
		Action action = m_Finished;
		m_Finished = null;
		action?.Invoke();
	}
}
