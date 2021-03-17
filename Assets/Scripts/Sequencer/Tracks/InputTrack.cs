using UnityEngine;

#if UNITY_EDITOR
public partial class InputTrack
{
	protected override float MinHeight => 110;
	protected override float MaxHeight => 110;
}
#endif

[CreateAssetMenu(fileName = "Input Track", menuName = "Tracks/Input Track")]
public partial class InputTrack : Track<InputClip>
{
	[SerializeField, Reference(typeof(InputReader))] string m_InputReader;
	[SerializeField]                                 float  m_Duration = 0.5f;
	[SerializeField, Range(0, 1)]                    float  m_Zone     = 0.8f;
	[SerializeField, Range(0, 1)]                    float  m_ZoneMin  = 0.7f;
	[SerializeField, Range(0, 1)]                    float  m_ZoneMax  = 0.9f;

	public override void Initialize(Sequencer _Sequencer)
	{
		base.Initialize(_Sequencer);
		
		InputReader inputReader = GetReference<InputReader>(m_InputReader);
		
		if (inputReader == null)
			Debug.LogError($"[{GetType().Name}] There is no input reader assigned to '{name}'", this);
		
		if (inputReader != null)
			inputReader.SetupZone(m_Zone, m_ZoneMin, m_ZoneMax);
		
		int inputID = 0;
		
		foreach (InputClip clip in Clips)
			clip.Initialize(Sequencer, inputReader, inputID++, m_Duration, m_Zone, m_ZoneMin, m_ZoneMax);
	}
}