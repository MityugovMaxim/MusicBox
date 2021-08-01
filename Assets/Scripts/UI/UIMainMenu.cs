using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIMainMenu : UIMenu, IInitializable, IDisposable
{
	[SerializeField] UIMainMenuTrack m_Track;
	[SerializeField] RectTransform   m_Container;
	[SerializeField] ScrollRect      m_Scroll;

	SignalBus               m_SignalBus;
	LevelProcessor          m_LevelProcessor;
	UIMainMenuTrack.Factory m_TrackFactory;

	readonly List<UIMainMenuTrack> m_Tracks = new List<UIMainMenuTrack>();

	string[] m_LevelIDs;

	[Inject]
	public void Construct(
		SignalBus               _SignalBus,
		LevelProcessor          _LevelProcessor,
		UIMainMenuTrack.Factory _TrackFactory
	)
	{
		m_SignalBus      = _SignalBus;
		m_LevelProcessor = _LevelProcessor;
		m_TrackFactory   = _TrackFactory;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelStartSignal>(RegisterLevelStart);
	}

	void RegisterLevelStart(LevelStartSignal _Signal)
	{
		Recenter(_Signal.LevelID);
		Hide(true);
	}

	protected override void Awake()
	{
		base.Awake();
		
		Show(true);
	}

	protected override void OnShowStarted()
	{
		RefreshPreviews();
	}

	void RefreshPreviews()
	{
		if (m_LevelProcessor == null)
			return;
		
		m_LevelIDs = m_LevelProcessor.GetLevelIDs();
		
		int delta = m_LevelIDs.Length - m_Tracks.Count;
		int count = Mathf.Abs(delta);
		
		if (delta > 0)
		{
			for (int i = 0; i < count; i++)
			{
				UIMainMenuTrack track = m_TrackFactory.Create(m_Track);
				track.RectTransform.SetParent(m_Container, false);
				m_Tracks.Add(track);
			}
		}
		else if (delta < 0)
		{
			for (int i = 0; i < count; i++)
			{
				int             index = m_Tracks.Count - 1;
				UIMainMenuTrack track = m_Tracks[index];
				Destroy(track.gameObject);
				m_Tracks.RemoveAt(index);
			}
		}
		
		foreach (UIMainMenuTrack track in m_Tracks)
			track.gameObject.SetActive(false);
		
		for (var i = 0; i < m_LevelIDs.Length; i++)
		{
			UIMainMenuTrack track   = m_Tracks[i];
			string          levelID = m_LevelIDs[i];
			
			track.Setup(levelID);
			
			track.gameObject.SetActive(true);
		}
	}

	void Recenter(string _LevelID)
	{
		if (string.IsNullOrEmpty(_LevelID))
		{
			Debug.LogErrorFormat("[UIMainMenu] Recenter failed. Level ID '{0}' is null or empty.", _LevelID);
			return;
		}
		
		UIMainMenuTrack track = m_Tracks.FirstOrDefault(_Track => _Track.gameObject.activeInHierarchy && _Track.LevelID == _LevelID);
		
		if (track == null)
		{
			Debug.LogErrorFormat("[UIMainMenu] Recenter failed. Track with level ID '{0}' not found.", _LevelID);
			return;
		}
		
		Rect source = track.GetWorldRect();
		Rect target = m_Scroll.content.GetWorldRect();
		
		float position = MathUtility.Remap01(source.yMin, target.yMin, target.yMax - source.height);
		
		m_Scroll.StopMovement();
		m_Scroll.verticalNormalizedPosition = position;
	}
}
