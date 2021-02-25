using UnityEditor;
using UnityEngine;

public class SequencerEditor : EditorWindow
{
	[MenuItem("Window/Sequencer")]
	public void Open()
	{
		SequencerEditor window = GetWindow<SequencerEditor>();
		window.minSize = new Vector2(300, 300);
	}

	void OnGUI()
	{
	}
}