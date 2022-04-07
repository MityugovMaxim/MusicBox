using AudioBox.Logging;

public class EditorHaptic : Haptic
{
	public override bool SupportsHaptic => false;

	protected override void Initialize() { }

	public override void Process(Type _Type)
	{
		Log.Info(this, "Haptic: {0}", _Type);
	}
}