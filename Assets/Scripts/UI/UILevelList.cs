using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UILevelList : UIEntity
{
	[SerializeField] UILevelElement[] m_LeftElements;
	[SerializeField] UILevelElement[] m_RightElements;
	[SerializeField] UILevelElement   m_CenterElement;
	[SerializeField] UILevelCollect   m_LevelCollect;
	[SerializeField] UILevelItems     m_LevelItems;
	[SerializeField] float            m_Duration = 0.2f;
	[SerializeField] AnimationCurve   m_Curve    = AnimationCurve.EaseInOut(0, 0, 1, 1);

	[SerializeField, Sound] string m_Sound;

	[Inject] ScoreManager      m_ScoreManager;
	[Inject] ScoresProcessor   m_ScoresProcessor;
	[Inject] ProfileProcessor  m_ProfileProcessor;
	[Inject] ProgressProcessor m_ProgressProcessor;
	[Inject] SoundProcessor    m_SoundProcessor;
	[Inject] HapticProcessor   m_HapticProcessor;

	string    m_SongID;
	int       m_Discs;
	ScoreRank m_SourceRank;
	ScoreRank m_TargetRank;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Discs      = m_ProfileProcessor.Discs;
		m_SourceRank = m_ScoresProcessor.GetRank(m_SongID);
		m_TargetRank = m_ScoreManager.GetRank();
		
		int level = m_ProfileProcessor.Level;
		
		m_LevelItems.Setup(level + 1);
		m_LevelItems.Show(true);
		
		SetLevel(level);
		
		ProcessPhase(0);
	}

	public async Task PlayAsync()
	{
		await Task.Delay(1000);
		
		int maxLevel = m_ProgressProcessor.GetMaxLevel(); 
		
		for (ScoreRank rank = m_SourceRank + 1; rank <= m_TargetRank; rank++)
		{
			int sourceLevel = m_ProgressProcessor.GetLevel(m_Discs);
			
			m_Discs++;
			
			int targetLevel = m_ProgressProcessor.GetLevel(m_Discs);
			
			await m_LevelCollect.CollectAsync(rank);
			
			await m_CenterElement.IncrementAsync();
			
			if (sourceLevel >= targetLevel)
				continue;
			
			await ShiftAsync();
			
			SetLevel(targetLevel);
			
			ProcessPhase(0);
			
			await m_LevelItems.PlayAsync();
			
			await Task.Delay(1000);
			
			if (targetLevel >= maxLevel)
				break;
			
			await m_LevelItems.HideAsync();
			
			m_LevelItems.Setup(targetLevel + 1);
			
			await m_LevelItems.ShowAsync();
		}
	}

	void SetLevel(int _Level)
	{
		int leftLevel = _Level - 1;
		foreach (UILevelElement left in m_LeftElements)
			left.Setup(leftLevel--, m_Discs);
		
		m_CenterElement.Setup(_Level, m_Discs);
		
		int rightLevel = _Level + 1;
		foreach (UILevelElement right in m_RightElements)
			right.Setup(rightLevel++, m_Discs);
	}

	void ProcessPhase(float _Phase)
	{
		m_CenterElement.Phase = _Phase;
		foreach (UILevelElement left in m_LeftElements)
			left.Phase = _Phase;
		foreach (UILevelElement right in m_RightElements)
			right.Phase = _Phase;
	}

	Task ShiftAsync()
	{
		m_SoundProcessor.Play(m_Sound);
		m_HapticProcessor.Process(Haptic.Type.ImpactRigid);
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		IEnumerator routine = ShiftRoutine(() => completionSource.TrySetResult(true));
		
		StartCoroutine(routine);
		
		return completionSource.Task;
	}

	IEnumerator ShiftRoutine(Action _Finished)
	{
		float time = 0;
		while (time < m_Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			float phase = m_Curve.Evaluate(time / m_Duration);
			
			ProcessPhase(phase);
		}
		
		_Finished?.Invoke();
	}
}