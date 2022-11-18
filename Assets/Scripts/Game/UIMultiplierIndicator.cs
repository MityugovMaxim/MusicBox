using UnityEngine;
using Zenject;

public class UIMultiplierIndicator : UIOrder
{
	[SerializeField] UIMultiplierProgress m_MultiplierProgress;
	[SerializeField] UIMultiplierLabel    m_MultiplierLabel;

	[SerializeField, Sound] string m_MultiplierX2Sound;
	[SerializeField, Sound] string m_MultiplierX4Sound;
	[SerializeField, Sound] string m_MultiplierX6Sound;
	[SerializeField, Sound] string m_MultiplierX8Sound;

	[Inject] ScoreController m_ScoreController;
	[Inject] SoundProcessor  m_SoundProcessor;

	int   m_Multiplier;
	float m_Progress;

	protected override void Awake()
	{
		base.Awake();
		
		m_Multiplier = 1;
		m_Progress   = 0;
		
		m_MultiplierLabel.Multiplier = m_Multiplier;
		
		m_MultiplierProgress.Progress(m_Progress, true);
		
		m_ScoreController.OnMultiplierChange += OnMultiplierChanged;
		m_ScoreController.OnProgressChange   += OnProgressChange;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_ScoreController.OnMultiplierChange -= OnMultiplierChanged;
		m_ScoreController.OnProgressChange   -= OnProgressChange;
	}

	void OnMultiplierChanged(int _Multiplier)
	{
		if (_Multiplier > m_Multiplier)
		{
			string multiplierSound = GetMultiplierSound(_Multiplier);
			
			m_MultiplierLabel.Multiplier = _Multiplier;
			
			m_SoundProcessor.Play(multiplierSound);
			
			m_MultiplierLabel.Play();
		}
		else if (_Multiplier < m_Multiplier)
		{
			m_MultiplierLabel.Restore();
			m_MultiplierLabel.Multiplier = _Multiplier;
		}
		
		m_Multiplier = _Multiplier;
	}

	void OnProgressChange(float _Progress)
	{
		if (_Progress > m_Progress)
		{
			m_MultiplierProgress.Progress(_Progress);
		}
		else if (_Progress < m_Progress)
		{
			m_MultiplierProgress.Progress(_Progress, true);
		}
		
		m_Progress = _Progress;
	}

	string GetMultiplierSound(int _Multiplier)
	{
		if (_Multiplier <= 2)
			return m_MultiplierX2Sound;
		else if (_Multiplier <= 4)
			return m_MultiplierX4Sound;
		else if (_Multiplier <= 6)
			return m_MultiplierX6Sound;
		else
			return m_MultiplierX8Sound;
	}
}
