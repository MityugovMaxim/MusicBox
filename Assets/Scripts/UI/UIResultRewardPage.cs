using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Functions;
using UnityEngine;
using Zenject;

public class UIResultRewardPage : UIResultMenuPage
{
	class ProgressData
	{
		public ScoreRank Rank   { get; }
		public float     Source { get; }
		public float     Target { get; }

		public ProgressData(ScoreRank _Rank, float _Source, float _Target)
		{
			Rank   = _Rank;
			Source = _Source;
			Target = _Target;
		}
	}

	public override ResultMenuPageType Type => ResultMenuPageType.Reward;

	[SerializeField] UICascadeTMPLabel  m_Title;
	[SerializeField] UIDiscProgress[]   m_DiscsProgress;
	[SerializeField] UIDiscs            m_Discs;
	[SerializeField] UIGroup            m_ContinueGroup;
	[SerializeField] UIGroup            m_LoaderGroup;
	[SerializeField] UILoader           m_Loader;
	[SerializeField] UICascadeUnitLabel m_ScoreLabel;
	[SerializeField] UICascadeUnitLabel m_CoinsLabel;
	[SerializeField] float              m_Duration = 1.5f;
	[SerializeField] AnimationCurve     m_Curve    = AnimationCurve.Linear(0, 0, 1, 1);

	ScoreProcessor    m_ScoreProcessor;
	LevelProcessor    m_LevelProcessor;
	MenuProcessor     m_MenuProcessor;
	HapticProcessor   m_HapticProcessor;
	LanguageProcessor m_LanguageProcessor;

	string      m_LevelID;
	ScoreRank   m_SourceRank;
	ScoreRank   m_TargetRank;
	int         m_SourceAccuracy;
	int         m_TargetAccuracy;
	long        m_Score;
	long        m_Coins;
	IEnumerator m_ScoreRoutine;
	IEnumerator m_CoinsRoutine;
	Action      m_ScoreFinished;
	Action      m_CoinsFinished;

	readonly Queue<ProgressData> m_ProgressData = new Queue<ProgressData>();

	[Inject]
	public void Construct(
		ScoreProcessor    _ScoreProcessor,
		LevelProcessor    _LevelProcessor,
		MenuProcessor     _MenuProcessor,
		HapticProcessor   _HapticProcessor,
		LanguageProcessor _LanguageProcessor
	)
	{
		m_ScoreProcessor    = _ScoreProcessor;
		m_LevelProcessor    = _LevelProcessor;
		m_MenuProcessor     = _MenuProcessor;
		m_HapticProcessor   = _HapticProcessor;
		m_LanguageProcessor = _LanguageProcessor;
	}

	public override void Setup(string _LevelID)
	{
		m_LevelID        = _LevelID;
		m_SourceRank     = m_ScoreProcessor.GetRank(m_LevelID);
		m_SourceAccuracy = m_ScoreProcessor.GetAccuracy(m_LevelID);
		m_TargetRank     = m_ScoreProcessor.Rank;
		m_TargetAccuracy = m_ScoreProcessor.Accuracy;
		
		m_ProgressData.Clear();
		for (ScoreRank rank = m_SourceRank; rank <= m_TargetRank; rank++)
		{
			if (rank >= ScoreRank.Platinum)
				break;
			
			int minAccuracy = m_ScoreProcessor.GetRankMinAccuracy(rank);
			int maxAccuracy = m_ScoreProcessor.GetRankMaxAccuracy(rank);
			
			m_ProgressData.Enqueue(
				new ProgressData(
					rank,
					Mathf.InverseLerp(minAccuracy, maxAccuracy, m_SourceAccuracy),
					Mathf.InverseLerp(minAccuracy, maxAccuracy, m_TargetAccuracy)
				)
			);
		}
		
		foreach (UIDiscProgress discProgress in m_DiscsProgress)
			discProgress.Hide(true);
		
		UIDiscProgress sourceDiscProgress = GetDiscProgress(m_SourceRank + 1);
		if (sourceDiscProgress != null)
		{
			int   minAccuracy = m_ScoreProcessor.GetRankMinAccuracy(m_SourceRank);
			int   maxAccuracy = m_ScoreProcessor.GetRankMaxAccuracy(m_SourceRank);
			float progress    = Mathf.InverseLerp(minAccuracy, maxAccuracy, m_SourceAccuracy);
			sourceDiscProgress.Setup(progress, progress);
			sourceDiscProgress.Show(true);
		}
		
		m_Score = m_ScoreProcessor.Score;
		m_Coins = m_LevelProcessor.GetPayout(m_LevelID, m_TargetRank);
		
		m_ScoreLabel.Value = 0;
		m_CoinsLabel.Value = 0;
		
		ProcessTitle();
		
		m_Discs.Hide(true);
		m_ContinueGroup.Hide(true);
		m_LoaderGroup.Hide(true);
	}

	public override async void Play()
	{
		await PlayRank();
		
		await Task.Delay(250);
		
		await PlayScore();
		
		await Task.Delay(250);
		
		await PlayCoins();
		
		await Task.Delay(250);
		
		m_ContinueGroup.Show();
	}

	public async void Continue()
	{
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		m_ContinueGroup.Hide();
		m_LoaderGroup.Show();
		
		m_Loader.Restore();
		m_Loader.Play();
		
		await FinishLevel();
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
		
		UIResultMenu resultMenu = m_MenuProcessor.GetMenu<UIResultMenu>();
		if (resultMenu == null)
			return;
		
		ResultMenuPageType pageType = m_TargetRank > m_SourceRank
			? ResultMenuPageType.Level
			: ResultMenuPageType.Control;
		
		await resultMenu.Select(pageType);
		
		resultMenu.Play(pageType);
	}

	UIDiscProgress GetDiscProgress(ScoreRank _Rank)
	{
		return m_DiscsProgress.FirstOrDefault(_DiscProgress => _DiscProgress.Rank == _Rank);
	}

	async Task PlayRank()
	{
		await m_Title.PlayAsync();
		
		while (m_ProgressData.Count > 0)
		{
			ProgressData progressData = m_ProgressData.Dequeue();
			
			UIDiscProgress discProgress = GetDiscProgress(progressData.Rank + 1);
			
			if (discProgress == null)
				continue;
			
			discProgress.Setup(progressData.Source, progressData.Target);
			
			await discProgress.ShowAsync();
			
			await discProgress.Progress();
			
			if (m_TargetRank <= progressData.Rank)
				break;
			
			discProgress.Collect();
		}
		
		if (m_TargetRank > ScoreRank.None)
		{
			foreach (UIDiscProgress discProgress in m_DiscsProgress)
				discProgress.Hide();
			
			m_Discs.Rank = m_TargetRank;
			
			await m_Discs.ShowAsync();
		}
	}

	Task PlayScore()
	{
		if (m_ScoreRoutine != null)
			StopCoroutine(m_ScoreRoutine);
		
		InvokeScoreFinished();
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		m_ScoreFinished = () => completionSource.SetResult(true);
		
		if (gameObject.activeInHierarchy)
		{
			m_ScoreRoutine = UnitRoutine(m_ScoreLabel, m_Score, InvokeScoreFinished);
			
			StartCoroutine(m_ScoreRoutine);
		}
		else
		{
			m_ScoreLabel.Value = m_Score;
			
			InvokeScoreFinished();
		}
		
		return completionSource.Task;
	}

	Task PlayCoins()
	{
		if (m_CoinsRoutine != null)
			StopCoroutine(m_CoinsRoutine);
		
		InvokeCoinsFinished();
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		m_CoinsFinished = () => completionSource.SetResult(true);
		
		if (gameObject.activeInHierarchy)
		{
			m_CoinsRoutine = UnitRoutine(m_CoinsLabel, m_Coins, InvokeCoinsFinished);
			
			StartCoroutine(m_CoinsRoutine);
		}
		else
		{
			m_CoinsLabel.Value = m_Coins;
			
			InvokeCoinsFinished();
		}
		
		return completionSource.Task;
	}

	void ProcessTitle()
	{
		m_Title.Text = m_TargetAccuracy > m_SourceAccuracy
			? m_LanguageProcessor.Get("RESULT_NEW_RECORD")
			: m_LanguageProcessor.Get("RESULT_TITLE");
	}

	void InvokeCoinsFinished()
	{
		Action action = m_CoinsFinished;
		m_CoinsFinished = null;
		action?.Invoke();
	}

	void InvokeScoreFinished()
	{
		Action action = m_ScoreFinished;
		m_ScoreFinished = null;
		action?.Invoke();
	}

	IEnumerator UnitRoutine(
		UICascadeUnitLabel _Label,
		double             _Value,
		Action             _Finished
	)
	{
		if (_Label == null)
		{
			_Finished?.Invoke();
			yield break;
		}
		
		_Label.Value = 0;
		
		if (_Value > 0 && m_Duration > float.Epsilon)
		{
			m_HapticProcessor.Play(Haptic.Type.ImpactLight, 30, m_Duration);
			
			float time = 0;
			while (time < m_Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				float phase = m_Curve.Evaluate(time / m_Duration);
				
				_Label.Value = (long)(_Value * phase);
			}
			
			_Label.RectTransform.SetAsLastSibling();
			_Label.Play();
		}
		
		m_HapticProcessor.Process(Haptic.Type.ImpactSoft);
		
		_Label.Value = (long)_Value;
		
		_Finished?.Invoke();
	}

	async Task<bool> FinishLevel()
	{
		HttpsCallableReference finishLevel = FirebaseFunctions.DefaultInstance.GetHttpsCallable("FinishLevel");
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		data["level_id"] = m_LevelID;
		data["rank"]     = (int)m_ScoreProcessor.Rank;
		data["accuracy"] = m_ScoreProcessor.Accuracy;
		data["score"]    = m_ScoreProcessor.Score;
		
		bool success;
		
		try
		{
			HttpsCallableResult result = await finishLevel.CallAsync(data);
			
			success = (bool)result.Data;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			
			success = false;
		}
		
		return success;
	}
}
