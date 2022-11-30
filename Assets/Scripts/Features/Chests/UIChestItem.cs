using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UIChestItem : UIEntity
{
	static readonly int m_RestoreParameterID  = Animator.StringToHash("Restore");
	static readonly int m_ProgressParameterID = Animator.StringToHash("Progress");
	static readonly int m_CollectParameterID  = Animator.StringToHash("Collect");

	[SerializeField] UIChestDisc     m_Disc;
	[SerializeField] UIChestProgress m_Progress;
	[SerializeField] UIChestBody     m_Body;
	[SerializeField] UIFlare         m_Flare;

	Animator m_Animator;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		Restore();
	}

	public void Setup(string _ChestID)
	{
		m_Disc.ChestID     = _ChestID;
		m_Progress.ChestID = _ChestID;
		m_Body.ChestID     = _ChestID;
	}

	public async void Progress()
	{
		m_Animator.SetTrigger(m_ProgressParameterID);
		
		bool complete = await m_Progress.ProgressAsync();
		
		if (!complete)
			return;
		
		m_Flare.Play();
		
		m_Animator.SetTrigger(m_CollectParameterID);
	}

	void Restore()
	{
		m_Animator.ResetTrigger(m_ProgressParameterID);
		m_Animator.ResetTrigger(m_CollectParameterID);
		m_Animator.SetTrigger(m_RestoreParameterID);
		m_Animator.Update(0);
	}
}
