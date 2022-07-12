using System;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.ASF;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class TutorialPlayer : ASFPlayer
{
	[Preserve]
	public class Factory : PlaceholderFactory<TutorialPlayer, TutorialPlayer> { }

	public override double Length => 28;

	[SerializeField] UIInputReceiver m_InputReceiver;
	[SerializeField] UIColorTrack    m_ColorTrack;
	[SerializeField] UITapTrack      m_TapTrack;
	[SerializeField] UIDoubleTrack   m_DoubleTrack;
	[SerializeField] UIHoldTrack     m_HoldTrack;

	[SerializeField] UITutorialOverlay m_InputOverlay;
	[SerializeField] UITutorialOverlay m_ComboOverlay;
	[SerializeField] UITutorialLabel   m_TapLabel;
	[SerializeField] UITutorialLabel   m_DoubleLabel;
	[SerializeField] UITutorialLabel   m_HoldLabel;
	[SerializeField] UITutorialLabel   m_BendLabel;
	[SerializeField] UITutorialLabel   m_CompleteLabel;

	[SerializeField, Sound] string m_OverlaySound;
	[SerializeField, Sound] string m_TapSuccessSound;
	[SerializeField, Sound] string m_DoubleSuccessSound;
	[SerializeField, Sound] string m_HoldHitSound;
	[SerializeField, Sound] string m_HoldSuccessSound;

	[Inject] ScoreManager m_ScoreManager;

	Action m_Finished;

	CancellationTokenSource m_TokenSource;

	bool m_Input;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = null;
	}

	void LateUpdate()
	{
		if (m_Input)
			m_InputReceiver.Sample();
	}

	public void Setup(float _Ratio, float _Speed, Action _Finished)
	{
		Rect rect = GetLocalRect();
		
		Ratio    = _Ratio;
		Duration = rect.height / _Speed;
		
		m_Finished = _Finished;
		
		m_InputOverlay.Setup(Ratio);
		m_InputReceiver.Setup(Ratio);
		
		AddTrack(new ASFTapTrack(m_TapTrack));
		AddTrack(new ASFDoubleTrack(m_DoubleTrack));
		AddTrack(new ASFHoldTrack(m_HoldTrack));
		AddTrack(new ASFHoldTrack(m_HoldTrack));
		AddTrack(new ASFColorTrack(m_ColorTrack, m_ColorTrack));
	}

	public async void Process()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		token.ThrowIfCancellationRequested();
		
		try
		{
			bool tapSuccess = await TapTutorialAsync(token);
			if (!tapSuccess)
				await TapTutorialAsync(token);
			
			bool doubleSuccess = await DoubleTutorialAsync(token);
			if (!doubleSuccess)
				await DoubleTutorialAsync(token);
			
			bool holdSuccess = await HoldTutorialAsync(token);
			if (!holdSuccess)
				await HoldTutorialAsync(token);
			
			bool bendSuccess = await BendTutorialAsync(token);
			if (!bendSuccess)
				await BendTutorialAsync(token);
		}
		catch (TaskCanceledException)
		{
			return;
		}
		finally
		{
			m_Finished?.Invoke();
		}
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	public void Clear()
	{
		m_TapTrack.Clear();
		m_DoubleTrack.Clear();
		m_HoldTrack.Clear();
		m_ColorTrack.Clear();
	}

	async Task<bool> TapTutorialAsync(CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			throw new TaskCanceledException("Tap tutorial canceled.");
		
		AddClips(
			new ASFTapClip(0, GetPosition(4)),   // 1
			new ASFTapClip(1.5, GetPosition(4)), // 2
			new ASFTapClip(3, GetPosition(3)),   // 3
			new ASFTapClip(4.5, GetPosition(2)), // 4
			new ASFTapClip(6, GetPosition(1)),   // 5
			new ASFTapClip(7.5, GetPosition(2)), // 6
			new ASFTapClip(9, GetPosition(3)),   // 7
			new ASFTapClip(10.5, GetPosition(4)) // 8
		);
		
		AddTapColors(10.5f);
		
		int success = 0;
		
		void ComboChanged(int _Combo, ScoreGrade _Grade)
		{
			if (_Grade != ScoreGrade.Fail && _Grade != ScoreGrade.Miss)
				success++;
		}
		
		m_Input = false;
		
		m_ScoreManager.OnComboChanged += ComboChanged;
		
		await SampleAsync(Duration, _Token);
		
		await m_InputOverlay.ShowAsync(_Token);
		
		m_TapLabel.Show();
		
		await InputAsync(_Token);
		
		m_TapLabel.Hide();
		
		await Task.WhenAll(
			m_InputOverlay.HideAsync(_Token),
			SampleAsync(10.5f + Duration, _Token)
		);
		
		m_ScoreManager.OnComboChanged -= ComboChanged;
		
		m_Input = false;
		
		return success >= 6;
	}

	async Task<bool> DoubleTutorialAsync(CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			throw new TaskCanceledException("Double tutorial canceled.");
		
		AddClips(
			new ASFDoubleClip(0),  // 1
			new ASFDoubleClip(2),  // 2
			new ASFDoubleClip(4),  // 3
			new ASFDoubleClip(6),  // 4
			new ASFDoubleClip(8),  // 5
			new ASFDoubleClip(10), // 6
			new ASFDoubleClip(12), // 7
			new ASFDoubleClip(14)  // 8
		);
		
		AddDoubleColors(14);
		
		int success = 0;
		
		void ComboChanged(int _Combo, ScoreGrade _Grade)
		{
			if (_Grade != ScoreGrade.Fail && _Grade != ScoreGrade.Miss)
				success++;
		}
		
		m_Input = false;
		
		m_ScoreManager.OnComboChanged += ComboChanged;
		
		await SampleAsync(Duration, _Token);
		
		await m_InputOverlay.ShowAsync(_Token);
		
		m_DoubleLabel.Show();
		
		await InputAsync(_Token);
		
		m_DoubleLabel.Hide();
		
		await Task.WhenAll(
			m_InputOverlay.HideAsync(_Token),
			SampleAsync(14 + Duration, _Token)
		);
		
		m_ScoreManager.OnComboChanged -= ComboChanged;
		
		m_Input = false;
		
		return success >= 6;
	}

	async Task<bool> HoldTutorialAsync(CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			throw new TaskCanceledException("Hold tutorial canceled.");
		
		AddClips(
			new ASFHoldClip(0,   2, new ASFHoldClip.Key(0, GetPosition(4)), new ASFHoldClip.Key(2, GetPosition(4))), // 1
			new ASFHoldClip(3,   5, new ASFHoldClip.Key(0, GetPosition(4)), new ASFHoldClip.Key(2, GetPosition(4))), // 2
			new ASFHoldClip(6,   8, new ASFHoldClip.Key(0, GetPosition(3)), new ASFHoldClip.Key(2, GetPosition(3))), // 3
			new ASFHoldClip(9,  11, new ASFHoldClip.Key(0, GetPosition(2)), new ASFHoldClip.Key(2, GetPosition(2))), // 4
			new ASFHoldClip(12, 14, new ASFHoldClip.Key(0, GetPosition(1)), new ASFHoldClip.Key(2, GetPosition(1)))  // 5
		);
		
		AddHoldColors(14);
		
		int success = 0;
		
		void ComboChanged(int _Combo, ScoreGrade _Grade)
		{
			if (_Grade != ScoreGrade.Fail && _Grade != ScoreGrade.Miss)
				success++;
		}
		
		m_Input = false;
		
		m_ScoreManager.OnComboChanged += ComboChanged;
		
		await SampleAsync(Duration, _Token);
		
		await m_InputOverlay.ShowAsync(_Token);
		
		m_HoldLabel.Show();
		
		await InputAsync(_Token);
		
		m_HoldLabel.Hide();
		
		await Task.WhenAll(
			m_InputOverlay.HideAsync(_Token),
			SampleAsync(14 + Duration, _Token)
		);
		
		m_ScoreManager.OnComboChanged -= ComboChanged;
		
		m_Input = false;
		
		return success >= 3;
	}

	async Task<bool> BendTutorialAsync(CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			throw new TaskCanceledException("Bend tutorial canceled.");
		
		AddClips(
			new ASFHoldClip(0,   2, new ASFHoldClip.Key(0, GetPosition(4)), new ASFHoldClip.Key(2, GetPosition(3))), // 1
			new ASFHoldClip(3,   5, new ASFHoldClip.Key(0, GetPosition(4)), new ASFHoldClip.Key(2, GetPosition(3))), // 2
			new ASFHoldClip(6,   8, new ASFHoldClip.Key(0, GetPosition(3)), new ASFHoldClip.Key(2, GetPosition(2))), // 3
			new ASFHoldClip(9,  11, new ASFHoldClip.Key(0, GetPosition(2)), new ASFHoldClip.Key(2, GetPosition(1))), // 4
			new ASFHoldClip(12, 14, new ASFHoldClip.Key(0, GetPosition(1)), new ASFHoldClip.Key(2, GetPosition(2))), // 5
			new ASFHoldClip(15, 17, new ASFHoldClip.Key(0, GetPosition(2)), new ASFHoldClip.Key(2, GetPosition(3))), // 6
			new ASFHoldClip(18, 20, new ASFHoldClip.Key(0, GetPosition(3)), new ASFHoldClip.Key(2, GetPosition(4)))  // 7
		);
		
		AddBendColors(20);
		
		int success = 0;
		
		void ComboChanged(int _Combo, ScoreGrade _Grade)
		{
			if (_Grade != ScoreGrade.Fail && _Grade != ScoreGrade.Miss)
				success++;
		}
		
		m_Input = false;
		
		m_ScoreManager.OnComboChanged += ComboChanged;
		
		await SampleAsync(Duration, _Token);
		
		await m_InputOverlay.ShowAsync(_Token);
		
		m_BendLabel.Show();
		
		await InputAsync(_Token);
		
		m_BendLabel.Hide();
		
		await Task.WhenAll(
			m_InputOverlay.HideAsync(_Token),
			SampleAsync(20 + Duration, _Token)
		);
		
		m_ScoreManager.OnComboChanged -= ComboChanged;
		
		m_Input = false;
		
		return success >= 3;
	}

	static float GetPosition(int _Position)
	{
		const int min = 1;
		const int max = 4;
		return Mathf.InverseLerp(min, max, _Position);
	}

	void AddClips<TClip>(params TClip[] _Clips) where TClip : ASFClip
	{
		ASFTrack<TClip> track = GetTrack<ASFTrack<TClip>>();
		
		double time = Time + Duration;
		foreach (TClip clip in _Clips)
		{
			clip.MinTime += time;
			clip.MaxTime += time;
			track.AddClip(clip);
		}
		
		Sample();
	}

	void AddTapColors(float _Duration)
	{
		AddClips(
			new ASFColorClip(
				0,
				new Color(0, 1, 0.87f),
				new Color(1, 0.25f, 0.5f),
				new Color(1, 1, 1, 0.75f),
				new Color(1, 0.25f, 0.5f)
			),
			new ASFColorClip(
				_Duration,
				new Color(0, 1, 0.87f),
				new Color(1, 0.25f, 0.5f),
				new Color(1, 1, 1, 0.75f),
				new Color(1, 0.25f, 0.5f)
			)
		);
	}

	void AddDoubleColors(float _Duration)
	{
		AddClips(
			new ASFColorClip(
				0,
				new Color(0, 1, 0.87f),
				new Color(1, 0.25f, 0.5f),
				new Color(1, 1, 1, 0.75f),
				new Color(1, 0.25f, 0.5f)
			),
			new ASFColorClip(
				_Duration,
				new Color(0, 1, 0.87f),
				new Color(1, 0.25f, 0.5f),
				new Color(1, 1, 1, 0.75f),
				new Color(1, 0.25f, 0.5f)
			)
		);
	}

	void AddHoldColors(float _Duration)
	{
		AddClips(
			new ASFColorClip(
				0,
				new Color(0, 1, 0.87f),
				new Color(1, 0.25f, 0.5f),
				new Color(1, 1, 1, 0.75f),
				new Color(1, 0.25f, 0.5f)
			),
			new ASFColorClip(
				_Duration,
				new Color(0, 1, 0.87f),
				new Color(1, 0.25f, 0.5f),
				new Color(1, 1, 1, 0.75f),
				new Color(1, 0.25f, 0.5f)
			)
		);
	}

	void AddBendColors(float _Duration)
	{
		AddClips(
			new ASFColorClip(
				0,
				new Color(0, 1, 0.87f),
				new Color(1, 0.25f, 0.5f),
				new Color(1, 1, 1, 0.75f),
				new Color(1, 0.25f, 0.5f)
			),
			new ASFColorClip(
				_Duration,
				new Color(0, 1, 0.87f),
				new Color(1, 0.25f, 0.5f),
				new Color(1, 1, 1, 0.75f),
				new Color(1, 0.25f, 0.5f)
			)
		);
	}

	Task SampleAsync(float _Duration, CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			return Task.FromCanceled(_Token);
		
		double source = Time;
		double target = Time + _Duration;
		
		return UnityTask.Phase(
			_Phase => Time = MathUtility.Lerp(source, target, _Phase),
			_Duration,
			_Token
		);
	}

	Task InputAsync(CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			return Task.FromCanceled(_Token);
		
		bool selected = false;
		
		void Select(UIHandle _Handle) => selected = _Handle != null;
		
		m_InputReceiver.OnSelect += Select;
		
		m_Input = true;
		
		return UnityTask.Until(
			() =>
			{
				if (!selected)
					return false;
				
				m_InputReceiver.OnSelect -= Select;
				
				return true;
			},
			_Token
		);
	}
}