using System;
using TMPro;
using UnityEngine;
using Zenject;

public class UITrackInfo : MonoBehaviour
{
	[SerializeField] TMP_Text m_Label;

	SignalBus      m_SignalBus;
	LevelProcessor m_LevelProcessor;

	[Inject]
	public void Construct(SignalBus _SignalBus, LevelProcessor _LevelProcessor)
	{
		m_SignalBus      = _SignalBus;
		m_LevelProcessor = _LevelProcessor;
		
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
	}

	void RegisterLevelStart(LevelStartSignal _Signal)
	{
		if (m_Label == null)
			return;
		
		string levelID = _Signal.LevelID;
		
		string title  = m_LevelProcessor.GetTitle(levelID);
		string artist = m_LevelProcessor.GetArtist(levelID);
		
		m_Label.text = $"<b>{title}</b>\n<size=26><color=#a0a0a0>{artist}</color></size>";
	}
}
