using UnityEngine;

public class UISongCreateRhythmPage : UIGroup
{
	public virtual bool Valid => true;

	public float Speed { get; private set; }
	public float BPM   { get; private set; }
	public float Bar   { get; private set; }

	[SerializeField] UIFloatField  m_BPMField;
	[SerializeField] UIFloatField  m_BarField;
	[SerializeField] UIFloatSlider m_SpeedSlider;

	protected override void Awake()
	{
		base.Awake();
		
		m_SpeedSlider.Setup(this, nameof(Speed));
		m_BPMField.Setup(this, nameof(BPM));
		m_BarField.Setup(this, nameof(Bar));
	}
}