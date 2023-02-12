using AudioBox.ASF;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UISongToolbar : UIEntity
{
	[SerializeField] Toggle m_SnapToggle;
	[SerializeField] Toggle m_BeatToggle;
	[SerializeField] Button m_PlayButton;
	[SerializeField] Button m_StepForwardButton;
	[SerializeField] Button m_StepBackwardButton;

	[Inject] UIPlayer     m_Player;
	[Inject] UIBeat       m_Beat;
	[Inject] UIBeatHandle m_BeatHandle;

	protected override void Awake()
	{
		base.Awake();
		
		m_SnapToggle.onValueChanged.AddListener(ToggleSnap);
		m_BeatToggle.onValueChanged.AddListener(ToggleBeat);
		
		m_PlayButton.onClick.AddListener(DisableTools);
		
		m_StepForwardButton.onClick.AddListener(StepForward);
		m_StepBackwardButton.onClick.AddListener(StepBackward);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_SnapToggle.onValueChanged.RemoveAllListeners();
		m_BeatToggle.onValueChanged.RemoveAllListeners();
		
		m_PlayButton.onClick.RemoveListener(DisableTools);
		
		m_StepForwardButton.onClick.RemoveAllListeners();
		m_StepBackwardButton.onClick.RemoveAllListeners();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		m_SnapToggle.isOn = true;
		m_BeatToggle.isOn = false;
	}

	void DisableTools()
	{
		m_BeatToggle.isOn = false;
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
}
