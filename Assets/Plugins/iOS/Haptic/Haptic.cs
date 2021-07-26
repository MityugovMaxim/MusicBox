using Zenject;

public abstract class Haptic
{
	public enum Type
	{
		Default      = 0,
		Selection    = 1,
		Success      = 2,
		Warning      = 3,
		Failure      = 4,
		ImpactLight  = 5,
		ImpactMedium = 6,
		ImpactHeavy  = 7,
	}

	SignalBus m_SignalBus;

	public static Haptic Create()
	{
		#if UNITY_IOS && !UNITY_EDITOR
		Haptic haptic = new iOSHaptic();
		#else
		Haptic haptic = new EditorHaptic();
		#endif
		
		haptic.Initialize();
		
		return haptic;
	}

	public abstract void Process(Type _Type);

	protected abstract void Initialize();
}