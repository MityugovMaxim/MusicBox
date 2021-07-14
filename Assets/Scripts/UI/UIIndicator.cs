public abstract class UIIndicator : UIEntity
{
	public abstract UIHandle Handle { get; }

	public abstract float MinPadding { get; }
	public abstract float MaxPadding { get; }
}