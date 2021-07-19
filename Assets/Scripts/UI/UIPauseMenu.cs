using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIPauseMenu : UIMenu
{
	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		// TODO: Load actual settings
		// TODO: Draw actual settings
	}

	protected override void OnHideStarted()
	{
		base.OnHideStarted();
		
		// TODO: Save actual settings
	}
}
