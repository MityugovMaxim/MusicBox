using UnityEngine;

public class UIOverlayButton : UIButton
{
	[SerializeField] UIGroup m_Overlay;

	protected override void OnNormal()
	{
		m_Overlay.Hide();
	}

	protected override void OnPress()
	{
		m_Overlay.Show();
	}

	protected override void OnClick()
	{
		m_Overlay.Hide();
	}
}