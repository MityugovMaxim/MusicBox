using UnityEngine;

public abstract class UISpectrum : UIEntity
{
	protected override void Awake()
	{
		base.Awake();
		
		Reposition();
	}

	#if UNITY_EDITOR
	[ContextMenu("Reposition")]
	public void ManualReposition()
	{
		Reposition();
	}
	#endif

	public abstract void Reposition();

	public abstract void Sample(float[] _Amplitude);
}