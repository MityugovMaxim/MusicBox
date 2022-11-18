using System;
using UnityEngine;
using UnityEngine.Scripting;

public class UIIndicatorFX : UIOrder
{
	[Preserve]
	public class Pool : UIEntityPool<UIIndicatorFX> { }

	public override int Thickness => 1;

	[Serializable]
	public class FX
	{
		[SerializeField] ParticleSystem[] m_FXs;

		public void Play()
		{
			foreach (ParticleSystem fx in m_FXs)
				fx.Play(true);
		}
	}

	[SerializeField] FX m_Perfect;
	[SerializeField] FX m_Great;
	[SerializeField] FX m_Good;
	[SerializeField] FX m_Bad;

	public void Play(ScoreGrade _ScoreGrade)
	{
		switch (_ScoreGrade)
		{
			case ScoreGrade.Perfect:
				m_Perfect.Play();
				break;
			case ScoreGrade.Great:
				m_Great.Play();
				break;
			case ScoreGrade.Good:
				m_Good.Play();
				break;
			case ScoreGrade.Bad:
				m_Bad.Play();
				break;
		}
	}
}