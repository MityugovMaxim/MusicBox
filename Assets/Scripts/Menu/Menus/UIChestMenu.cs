using UnityEngine;
using UnityEngine.UI;

[Menu(MenuType.ChestMenu)]
public class UIChestMenu : UIMenu
{
	[SerializeField] UIChestReward m_Reward;
	[SerializeField] UIGroup       m_LoaderGroup;
	[SerializeField] Button        m_ProcessButton;

	bool m_Processed;
	bool m_Loading;
	bool m_Opened;

	protected override void Awake()
	{
		base.Awake();
		
		m_ProcessButton.Subscribe(Process);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_ProcessButton.Subscribe(Process);
	}

	public void Setup(RankType _Rank)
	{
		m_Reward.Setup(_Rank);
	}

	public void Process(ChestReward _Reward)
	{
		m_Loading = false;
		
		m_Reward.Process(_Reward);
		
		m_LoaderGroup.Hide();
		
		if (m_Processed)
			Process();
	}

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		m_Loading   = true;
		m_Processed = false;
		m_Opened    = false;
		
		m_Reward.Restore();
		
		m_LoaderGroup.Hide(true);
	}

	protected override void OnShowFinished()
	{
		base.OnShowFinished();
		
		m_ProcessButton.interactable = true;
	}

	protected override void OnHideStarted()
	{
		base.OnHideStarted();
		
		m_ProcessButton.interactable = false;
	}

	async void Process()
	{
		if (m_Opened)
		{
			Hide();
			return;
		}
		
		m_Processed = true;
		
		if (m_Loading)
		{
			m_LoaderGroup.Show();
			return;
		}
		
		m_Opened = true;
		
		await m_Reward.PlayAsync();
	}
}
