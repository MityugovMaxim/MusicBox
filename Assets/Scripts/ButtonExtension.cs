using UnityEngine.Events;
using UnityEngine.UI;

public static class ButtonExtension
{
	public static void Subscribe(this UIRepeatButton _Button, UnityAction _Action)
	{
		if (_Button != null)
			_Button.AddListener(_Action);
	}

	public static void Unsubscribe(this UIRepeatButton _Button, UnityAction _Action)
	{
		if (_Button != null)
			_Button.RemoveListener(_Action);
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
}
