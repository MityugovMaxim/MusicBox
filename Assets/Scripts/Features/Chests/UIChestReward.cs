using System;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UIChestReward : UIEntity
{
	const string PLAY_STATE = "play";

	static readonly int m_PlayParameterID    = Animator.StringToHash("Play");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	[SerializeField] UIChestImage m_Image;
	[SerializeField] GameObject   m_CoinsContent;
	[SerializeField] GameObject   m_SongContent;
	[SerializeField] UICoinsItem  m_CoinsItem;
	[SerializeField] UISongItem   m_SongItem;

	Animator m_Animator;

	Action m_Finished;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.SubscribeComplete(PLAY_STATE, InvokeFinished);
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Animator.UnsubscribeComplete(PLAY_STATE, InvokeFinished);
	}

	public void Setup(RankType _Rank)
	{
		m_Image.Rank = _Rank;
	}

	public void Process(ChestReward _Reward)
	{
		m_CoinsContent.SetActive(false);
		m_SongContent.SetActive(false);
		
		if (!string.IsNullOrEmpty(_Reward.SongID))
		{
			m_SongContent.SetActive(true);
			m_SongItem.Setup(_Reward.SongID);
		}
		else if (!string.IsNullOrEmpty(_Reward.VoucherID))
		{
			//m_VoucherContent.SetActive(true);
			//m_VoucherItem.Setup(_Reward.VoucherID);
		}
		else if (_Reward.Coins > 0)
		{
			m_CoinsContent.SetActive(true);
			m_CoinsItem.Setup(_Reward.Coins);
		}
	}

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
	}

	void InvokeFinished()
	{
		Action action = m_Finished;
		m_Finished = null;
		action?.Invoke();
	}
}
