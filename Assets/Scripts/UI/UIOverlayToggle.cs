using System;
using UnityEngine;
using UnityEngine.Events;

public class UIOverlayToggle : UIButton
{
	[Serializable]
	public class ToggleEvent : UnityEvent<bool> { }

	public bool Value
	{
		get => m_Value;
		set
		{
			if (m_Value == value)
				return;
			
			m_Value = value;
			
			ProcessValue();
			
			ValueChanged?.Invoke(m_Value);
		}
	}

	public ToggleEvent ValueChanged => m_ValueChanged;

	[SerializeField] UIGroup     m_Overlay;
	[SerializeField] UIGroup     m_Active;
	[SerializeField] UIGroup     m_Inactive;
	[SerializeField] ToggleEvent m_ValueChanged;

	bool m_Value;

	public void SetState(bool _Value)
	{
		if (m_Value == _Value)
			return;
		
		m_Value = _Value;
		
		ProcessValue();
	}

	protected override void Awake()
	{
		base.Awake();
		
		ProcessValue(true);
	}

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
		
		Value = !Value;
	}

	async void ProcessValue(bool _Instant = false)
	{
		if (Value)
		{
			await m_Active.ShowAsync(_Instant);
			
			m_Inactive.Hide(true);
		}
		else
		{
			m_Inactive.Show(true);
			
			await m_Active.HideAsync(_Instant);
		}
	}
}