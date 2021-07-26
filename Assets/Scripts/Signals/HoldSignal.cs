public class HoldSignal
{
	public float MinProgress { get; }

	public float MaxProgress { get; }

	public float Progress => MaxProgress - MinProgress;

	public HoldSignal(float _MinProgress, float _MaxProgress)
	{
		MinProgress = _MinProgress;
		MaxProgress = _MaxProgress;
	}
}