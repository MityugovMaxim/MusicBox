using System.Collections.Generic;
using System.Linq;
using AudioBox.ASF;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UISongToolbar : UIEntity
{
	[SerializeField] Toggle m_SnapToggle;
	[SerializeField] Toggle m_TapToggle;
	[SerializeField] Toggle m_DoubleToggle;
	[SerializeField] Toggle m_HoldToggle;
	[SerializeField] Toggle m_ColorToggle;
	[SerializeField] Toggle m_BeatToggle;

	[SerializeField] Button m_PlayButton;

	[SerializeField] Button m_CopyButton;
	[SerializeField] Button m_PasteButton;
	[SerializeField] Button m_DeleteButton;

	[SerializeField] Button m_StepForwardButton;
	[SerializeField] Button m_StepBackwardButton;

	[Inject] UIPlayer             m_Player;
	[Inject] UIBeat               m_Beat;
	[Inject] UIBeatHandle         m_BeatHandle;
	[Inject] UICreateTapHandle    m_CreateTapHandle;
	[Inject] UICreateDoubleHandle m_CreateDoubleHandle;
	[Inject] UICreateHoldHandle   m_CreateHoldHandle;
	[Inject] UICreateColorHandle  m_CreateColorHandle;

	readonly List<ASFClip> m_Buffer = new List<ASFClip>();

	protected override void Awake()
	{
		base.Awake();
		
		m_SnapToggle.onValueChanged.AddListener(ToggleSnap);
		m_TapToggle.onValueChanged.AddListener(ToggleTapCreation);
		m_DoubleToggle.onValueChanged.AddListener(ToggleDoubleCreation);
		m_HoldToggle.onValueChanged.AddListener(ToggleHoldCreation);
		m_ColorToggle.onValueChanged.AddListener(ToggleColorCreation);
		m_BeatToggle.onValueChanged.AddListener(ToggleBeat);
		
		m_PlayButton.onClick.AddListener(DisableTools);
		
		m_CopyButton.onClick.AddListener(Copy);
		m_PasteButton.onClick.AddListener(Paste);
		m_DeleteButton.onClick.AddListener(Delete);
		
		m_StepForwardButton.onClick.AddListener(StepForward);
		m_StepBackwardButton.onClick.AddListener(StepBackward);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_SnapToggle.onValueChanged.RemoveAllListeners();
		m_TapToggle.onValueChanged.RemoveAllListeners();
		m_DoubleToggle.onValueChanged.RemoveAllListeners();
		m_HoldToggle.onValueChanged.RemoveAllListeners();
		m_ColorToggle.onValueChanged.RemoveAllListeners();
		m_BeatToggle.onValueChanged.RemoveAllListeners();
		
		m_PlayButton.onClick.RemoveListener(DisableTools);
		
		m_CopyButton.onClick.RemoveAllListeners();
		m_PasteButton.onClick.RemoveAllListeners();
		m_DeleteButton.onClick.RemoveAllListeners();
		
		m_StepForwardButton.onClick.RemoveAllListeners();
		m_StepBackwardButton.onClick.RemoveAllListeners();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		m_SnapToggle.isOn   = true;
		m_ColorToggle.isOn  = false;
		m_TapToggle.isOn    = false;
		m_DoubleToggle.isOn = false;
		m_HoldToggle.isOn   = false;
		m_BeatToggle.isOn   = false;
	}

	void DisableTools()
	{
		m_ColorToggle.isOn = false;
		m_TapToggle.isOn = false;
		m_DoubleToggle.isOn = false;
		m_HoldToggle.isOn = false;
		m_BeatToggle.isOn = false;
	}

	void Delete()
	{
		DeleteClips<ASFTapTrack, ASFTapClip>();
		DeleteClips<ASFDoubleTrack, ASFDoubleClip>();
		DeleteClips<ASFHoldTrack, ASFHoldClip>();
		DeleteClips<ASFColorTrack, ASFColorClip>();
		ClipSelection.Clear();
		
		m_Player.Sample();
	}

	public void Copy()
	{
		m_Buffer.Clear();
		CopyClips<ASFTapClip>();
		CopyClips<ASFDoubleClip>();
		CopyClips<ASFHoldClip>();
		CopyClips<ASFColorClip>();
	}

	public void Paste()
	{
		PasteClips<ASFTapTrack, ASFTapClip>();
		PasteClips<ASFDoubleTrack, ASFDoubleClip>();
		PasteClips<ASFHoldTrack, ASFHoldClip>();
		PasteClips<ASFColorTrack, ASFColorClip>();
		m_Player.Sample();
	}

	void StepForward()
	{
		if (m_Player.State != ASFPlayerState.Stop)
			return;
		
		m_Player.Time += 10;
	}

	void StepBackward()
	{
		if (m_Player.State != ASFPlayerState.Stop)
			return;
		
		m_Player.Time -= 10;
	}

	void ToggleBeat(bool _Value)
	{
		m_BeatHandle.gameObject.SetActive(_Value);
	}

	void ToggleSnap(bool _Value)
	{
		m_Beat.SnapActive = _Value;
	}

	void ToggleTapCreation(bool _Value)
	{
		m_CreateTapHandle.gameObject.SetActive(_Value);
	}

	void ToggleDoubleCreation(bool _Value)
	{
		m_CreateDoubleHandle.gameObject.SetActive(_Value);
	}

	void ToggleHoldCreation(bool _Value)
	{
		m_CreateHoldHandle.gameObject.SetActive(_Value);
	}

	void ToggleColorCreation(bool _Value)
	{
		m_CreateColorHandle.gameObject.SetActive(_Value);
	}

	void DeleteClips<TTrack, TClip>() where TTrack : ASFTrack<TClip> where TClip : ASFClip
	{
		TClip[] clips = ClipSelection.GetItems<TClip>();
		if (clips == null)
			return;
		
		TTrack track = m_Player.GetTrack<TTrack>();
		if (track == null)
			return;
		
		foreach (TClip clip in clips)
			track.RemoveClip(clip);
	}

	void CopyClips<TClip>() where TClip : ASFClip
	{
		TClip[] clips = ClipSelection.GetItems<TClip>();
		
		if (clips == null)
			return;
		
		foreach (TClip clip in clips)
			m_Buffer.Add(clip.Clone());
	}

	void PasteClips<TTrack, TClip>() where TTrack : ASFTrack<TClip> where TClip : ASFClip
	{
		if (m_Buffer.Count == 0)
			return;
		
		TTrack track = m_Player.GetTrack<TTrack>();
		
		if (track == null)
			return;
		
		double time   = m_Beat.Snap(m_Player.Time);
		double offset = m_Buffer.Min(_Clip => _Clip.MinTime);
		
		TClip[] clips = m_Buffer.OfType<TClip>().ToArray();
		
		foreach (TClip clip in clips)
		{
			TClip clone = clip.Clone() as TClip;
			
			if (clone == null)
				continue;
			
			clone.MinTime -= offset;
			clone.MaxTime -= offset;
			clone.MinTime += time;
			clone.MaxTime += time;
			track.AddClip(clone);
		}
		
		track.SortClips();
	}
}