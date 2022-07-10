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

	[Inject] ScoreManager   m_ScoreManager;
	[Inject] SoundProcessor m_SoundProcessor;

	int   m_Multiplier;
	float m_Progress;

	protected override void Awake()
	{
		base.Awake();
		
		m_Multiplier = 1;
		m_Progress   = 0;
		
		m_MultiplierLabel.Multiplier = m_Multiplier;
		
		m_MultiplierProgress.Progress(m_Progress, true);
		
		if (m_ScoreManager != null)
			m_ScoreManager.OnMultiplierChanged += OnMultiplierChanged;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		if (m_ScoreManager != null)
			m_ScoreManager.OnMultiplierChanged -= OnMultiplierChanged;
	}

	void OnMultiplierChanged(int _Multiplier, float _Progress)
	{
		if (_Multiplier > m_Multiplier)
		{
			string multiplierSound = GetMultiplierSound(_Multiplier);
			
			m_MultiplierLabel.Multiplier = _Multiplier;
			
			m_SoundProcessor.Play(multiplierSound);
			
			m_MultiplierProgress.Play(_Progress);
			
			m_MultiplierLabel.Play();
		}
		else if (_Multiplier < m_Multiplier)
		{
			m_MultiplierLabel.Restore();
			m_MultiplierLabel.Multiplier = _Multiplier;
			m_MultiplierProgress.Progress(_Progress, true);
		}
		
		if (_Progress > m_Progress)
		{
			m_MultiplierProgress.Progress(_Progress);
		}
		else if (_Progress < m_Progress)
		{
			m_MultiplierProgress.Progress(_Progress, true);
		}
		
		m_Progress   = _Progress;
		m_Multiplier = _Multiplier;
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