public class Haptic
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

	public static void Initialize()
	{
		if (m_Instance != null)
			return;
		
		#if !UNITY_EDITOR && UNITY_IOS
		m_Instance = new iOSHaptic();
		#else
		m_Instance = new Haptic();
		#endif
		
		m_Instance.InitializeInternal();
	}

	static Haptic m_Instance;

	public static void Process(Type _Type)
	{
		m_Instance.ProcessInternal(_Type);
	}

	protected virtual void InitializeInternal() { }

	protected virtual void ProcessInternal(Type _Type) { }
}