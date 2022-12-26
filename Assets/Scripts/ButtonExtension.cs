using System;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public static class ButtonExtension
{
	public static void Subscribe(this UIAdminField _Field, UnityAction<string> _Action)
	{
		if (_Field != null)
			_Field.AddListener(_Action);
	}

	public static void Unsubscribe(this UIAdminField _Field, UnityAction<string> _Action)
	{
		if (_Field != null)
			_Field.RemoveListener(_Action);
	}

	public static void Subscribe(this UIButton _Button, Action _Action)
	{
		if (_Button != null)
			_Button.Action.AddListener(_Action);
	}

	public static void Unsubscribe(this UIButton _Button, Action _Action)
	{
		if (_Button != null)
			_Button.Action.RemoveListener(_Action);
	}

	public static void Subscribe(this Slider _Slider, UnityAction<float> _Action)
	{
		if (_Slider != null)
			_Slider.onValueChanged.AddListener(_Action);
	}

	public static void Unsubscribe(this Slider _Slider, UnityAction<float> _Action)
	{
		if (_Slider != null)
			_Slider.onValueChanged.RemoveListener(_Action);
	}

	public static void Subscribe(this TMP_InputField _Field, UnityAction<string> _Action)
	{
		if (_Field != null)
			_Field.onEndEdit.AddListener(_Action);
	}

	public static void Unsubscribe(this TMP_InputField _Field, UnityAction<string> _Action)
	{
		if (_Field != null)
			_Field.onEndEdit.RemoveListener(_Action);
	}

	public static void Subscribe(this UIRepeatButton _Button, UnityAction _Action)
	{
		if (_Button != null)
			_Button.Subscribe(_Action);
	}

	public static void Unsubscribe(this UIRepeatButton _Button, UnityAction _Action)
	{
		if (_Button != null)
			_Button.Unsubscribe(_Action);
	}

	public static void Subscribe(this Button _Button, UnityAction _Action)
	{
		if (_Button != null)
			_Button.onClick.AddListener(_Action);
	}

	public static void Unsubscribe(this Button _Button, UnityAction _Action)
	{
		if (_Button != null)
			_Button.onClick.RemoveListener(_Action);
	}

	public static void Subscribe(this Toggle _Toggle, UnityAction<bool> _Action)
	{
		if (_Toggle != null)
			_Toggle.onValueChanged.AddListener(_Action);
	}

	public static void Unsubscribe(this Toggle _Toggle, UnityAction<bool> _Action)
	{
		if (_Toggle != null)
			_Toggle.onValueChanged.RemoveListener(_Action);
	}
}
