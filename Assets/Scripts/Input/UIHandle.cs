using System;
using UnityEngine;

public abstract class UIHandle : UIEntity
{
	public event Action OnSuccess;
	public event Action OnFail;

	public bool Select(Rect _Rect)
	{
		Rect area = RectTransform.InverseTransformRect(_Rect);
		Rect rect = RectTransform.rect;
		
		return rect.Overlaps(area);
	}

	public abstract void StartReceiveInput();
	public abstract void StopReceiveInput();
	public abstract void TouchDown(int _ID, Vector2 _Position);
	public abstract void TouchUp(int   _ID, Vector2 _Position);
	public abstract void TouchMove(int _ID, Vector2 _Position);

	protected void InvokeSuccess()
	{
		OnSuccess?.Invoke();
	}

	protected void InvokeFail()
	{
		OnFail?.Invoke();
	}
}