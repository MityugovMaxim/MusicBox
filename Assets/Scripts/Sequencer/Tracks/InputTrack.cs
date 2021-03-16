using JetBrains.Annotations;
using UnityEngine;

#if UNITY_EDITOR
public partial class InputTrack
{
	protected override float MinHeight => 110;
	protected override float MaxHeight => 110;

	[SerializeField, UsedImplicitly] float m_Duration = 0.5f;
}
#endif

[CreateAssetMenu(fileName = "Input Track", menuName = "Tracks/Input Track")]
public partial class InputTrack : Track<InputClip>
{
	[SerializeField, Reference(typeof(InputReader))] string m_InputReader;
	[SerializeField, Range(0, 1)]                    float  m_Time     = 0.8f;
	[SerializeField, Range(0, 1)]                    float  m_MinZone  = 0.7f;
	[SerializeField, Range(0, 1)]                    float  m_MaxZone  = 0.9f;

	public override void Initialize(Sequencer _Sequencer)
	{
		base.Initialize(_Sequencer);
		
		InputReader inputReader = GetReference<InputReader>(m_InputReader);
		
		if (inputReader == null)
			Debug.LogError($"[{GetType().Name}] There is no input reader assigned to '{name}'", this);
		
		int id = 0;
		
		foreach (InputClip clip in Clips)
			clip.Initialize(id++, inputReader, m_Time, m_MinZone, m_MaxZone);
	}
}