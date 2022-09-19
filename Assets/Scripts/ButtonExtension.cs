using UnityEngine.Events;
using UnityEngine.UI;

public static class ButtonExtension
{
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