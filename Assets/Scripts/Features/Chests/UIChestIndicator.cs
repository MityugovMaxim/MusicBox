using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

[ExecuteInEditMode]
public class UIChestIndicator : UIEntity
{
	[SerializeField] UIChestIcon m_Icon;
	[SerializeField] TMP_Text    m_Progress;
	[SerializeField] UIGroup     m_ProgressGroup;

	[Inject] ChestsInventory m_ChestsInventory;
	[Inject] ChestsManager   m_ChestsManager;

	[SerializeField] RankType m_ChestRank;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		m_ProgressGroup.Show(true);
		
		ProcessData();
		
		Subscribe();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		Unsubscribe();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (!IsInstanced || Application.isPlaying)
			return;
		
		ProcessIcon();
	}
	#endif

	public async Task ProgressAsync()
	{
		Unsubscribe();
		
		m_ProgressGroup.Show();
		
		int source = m_ChestsInventory.GetSource(m_ChestRank);
		int target = m_ChestsInventory.GetTarget(m_ChestRank);
		
		source = Mathf.Clamp(source + 1, 0, target);
		
		m_Progress.text = GetProgress(source, target);
		
		await m_Icon.ProgressAsync();
		
		Subscribe();
	}

	public async Task CollectAsync()
	{
		Unsubscribe();
		
		m_ProgressGroup.Hide();
		
		await m_Icon.CollectAsync();
		
		Subscribe();
	}

	void Subscribe()
	{
		m_ChestsInventory.Profile.Subscribe(DataEventType.Load, ProcessData);
		m_ChestsInventory.Profile.Subscribe(DataEventType.Add, ProcessData);
		m_ChestsInventory.Profile.Subscribe(DataEventType.Remove, ProcessData);
		m_ChestsInventory.Profile.Subscribe(DataEventType.Change, ProcessData);
		m_ChestsManager.Collection.Subscribe(DataEventType.Load, ProcessData);
		m_ChestsManager.Collection.Subscribe(DataEventType.Change, ProcessData);
	}

	void Unsubscribe()
	{
		m_ChestsInventory.Profile.Unsubscribe(DataEventType.Load, ProcessData);
		m_ChestsInventory.Profile.Unsubscribe(DataEventType.Add, ProcessData);
		m_ChestsInventory.Profile.Unsubscribe(DataEventType.Remove, ProcessData);
		m_ChestsInventory.Profile.Unsubscribe(DataEventType.Change, ProcessData);
		m_ChestsManager.Collection.Unsubscribe(DataEventType.Load, ProcessData);
		m_ChestsManager.Collection.Unsubscribe(DataEventType.Change, ProcessData);
	}

	void ProcessData()
	{
		ProcessIcon();
		
		ProcessProgress();
	}

	void ProcessIcon()
	{
		m_Icon.ChestRank = m_ChestRank;
	}

	void ProcessProgress()
	{
		int source = m_ChestsInventory.GetSource(m_ChestRank);
		int target = m_ChestsInventory.GetTarget(m_ChestRank);
		
		m_Progress.text = GetProgress(source, target);
	}

	string GetProgress(int _Source, int _Target)
	{
		string icon;
		switch (m_ChestRank)
		{
			case RankType.Bronze:
				icon = "<sprite name=disc_bronze>";
				break;
			case RankType.Silver:
				icon = "<sprite name=disc_silver>";
				break;
			case RankType.Gold:
				icon = "<sprite name=disc_gold>";
				break;
			case RankType.Platinum:
				icon = "<sprite name=disc_platinum>";
				break;
			default:
				icon = string.Empty;
				break;
		}
		
		return $"{icon}{_Source}/{_Target}";
	}
}
