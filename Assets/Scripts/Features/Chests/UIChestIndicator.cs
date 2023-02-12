using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

[ExecuteInEditMode]
public class UIChestIndicator : UIEntity
{
	[SerializeField] UIChestImage m_Image;
	[SerializeField] RankType     m_Rank;
	[SerializeField] TMP_Text     m_Progress;
	[SerializeField] UIGroup      m_ProgressGroup;

	[Inject] ChestsManager m_ChestsManager;


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
		
		ProcessImage();
	}
	#endif

	public async void Progress() => await ProgressAsync();

	public async Task ProgressAsync()
	{
		Unsubscribe();
		
		m_ProgressGroup.Show();
		
		int progress = m_ChestsManager.GetChestProgress(m_Rank);
		int capacity = m_ChestsManager.GetChestCapacity(m_Rank);
		
		progress = Mathf.Clamp(progress + 1, 0, capacity);
		
		m_Progress.text = GetProgress(progress, capacity);
		
		await m_Image.ProgressAsync();
		
		Subscribe();
	}

	public async Task CollectAsync()
	{
		Unsubscribe();
		
		m_ProgressGroup.Hide();
		
		await m_Image.CollectAsync();
		
		Subscribe();
	}

	void Subscribe()
	{
		m_ChestsManager.SubscribeChests(m_Rank, ProcessData);
		m_ChestsManager.Collection.Subscribe(DataEventType.Load, ProcessData);
		m_ChestsManager.Collection.Subscribe(DataEventType.Change, ProcessData);
	}

	void Unsubscribe()
	{
		m_ChestsManager.UnsubscribeChests(m_Rank, ProcessData);
		m_ChestsManager.Collection.Unsubscribe(DataEventType.Load, ProcessData);
		m_ChestsManager.Collection.Unsubscribe(DataEventType.Change, ProcessData);
	}

	void ProcessData()
	{
		ProcessImage();
		
		ProcessProgress();
	}

	void ProcessImage()
	{
		m_Image.Rank = m_Rank;
	}

	void ProcessProgress()
	{
		int progress = m_ChestsManager.GetChestProgress(m_Rank);
		int capacity = m_ChestsManager.GetChestCapacity(m_Rank);
		
		m_Progress.text = GetProgress(progress, capacity);
	}

	string GetProgress(int _Source, int _Target)
	{
		string icon;
		switch (m_Rank)
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
