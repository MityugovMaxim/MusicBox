using System;
using UnityEngine;

public abstract class UIHandle : UIEntity
{
	public event Action OnSuccess;
	public event Action OnFail;

	public virtual bool Select(Rect _Rect)
	{
		Rect area = RectTransform.InverseTransformRect(_Rect);
		Rect rect = RectTransform.rect;
		
		return rect.Overlaps(area);
	}

	public abstract void TouchDown(Vector2 _Position);
	public abstract void TouchMove(Vector2 _Position);
	public abstract void TouchUp(Vector2   _Position);

	public abstract void StartReceiveInput();

	public abstract void StopReceiveInput();

	protected void InvokeSuccess()
	{
		OnSuccess?.Invoke();
	}

	protected void InvokeFail()
	{
		OnFail?.Invoke();
	}
}