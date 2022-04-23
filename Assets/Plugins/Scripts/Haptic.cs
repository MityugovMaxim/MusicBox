public abstract class Haptic
{
	public enum Type
	{
		None         = 0,
		Selection    = 1,
		Success      = 2,
		Warning      = 3,
		Failure      = 4,
		ImpactLight  = 5,
		ImpactMedium = 6,
		ImpactHeavy  = 7,
		ImpactRigid  = 8,
		ImpactSoft   = 9,
	}

	public abstract bool SupportsHaptic { get; }

	public static Haptic Create()
	{
		#if UNITY_EDITOR
		Haptic haptic = new EditorHaptic();
		#elif UNITY_IOS
		Haptic haptic = new iOSHaptic();
		#elif UNITY_ANDROID
		Haptic haptic = new AndroidHaptic();
		#endif
		
		haptic.Initialize();
		
		return haptic;
	}

	protected abstract void Initialize();

	public abstract void Process(Type _Type);
}