using System;
using UnityEngine;

public abstract class UIHandle : UIEntity
{
	public abstract float Progress { get; }

	public event Action<float> OnSuccess;
	public event Action<float> OnFail;

	public bool Select(Rect _Rect)
	{
		Rect area = RectTransform.InverseTransformRect(_Rect);
		Rect rect = RectTransform.rect;
		
		return rect.Overlaps(area);
	}

	public abstract void StartReceiveInput();
	public abstract void StopReceiveInput();
	public abstract void TouchDown(int _ID, Rect _Area);
	public abstract void TouchUp(int   _ID, Rect _Area);
	public abstract void TouchMove(int _ID, Rect _Area);

	protected void InvokeSuccess()
	{
		OnSuccess?.Invoke(Progress);
	}

	protected void InvokeFail()
	{
		OnFail?.Invoke(Progress);
	}
}