using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(AlphaClip))]
public class AlphaClipDrawer : ClipDrawer
{
	AnimationCurve AlphaCurve { get; }

	public AlphaClipDrawer(Clip _Clip) : base(_Clip)
	{
		if (_Clip is AlphaClip clip)
			AlphaCurve = clip.AlphaCurve;
	}

	protected override void DrawContent()
	{
		if (Event.current.type != EventType.Repaint)
			return;
		
		EditorGUI.CurveField(ClipRect, AlphaCurve);
	}
}
