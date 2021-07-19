using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIResultMenu : UIMenu
{
	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		// TODO: Get result data
		// TODO: Draw result data
	}

	protected override void OnShowFinished()
	{
		base.OnShowFinished();
		
		// TODO: Animate result data
		// TODO: Select hide option (Retry, MainMenu)
	}

	protected override void OnHideStarted()
	{
		base.OnHideStarted();
		
		// TODO: Apply hide option
	}
}