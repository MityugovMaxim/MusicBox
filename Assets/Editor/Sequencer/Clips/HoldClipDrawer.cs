[SequencerDrawer(typeof(HoldClip))]
public class HoldClipDrawer : ClipDrawer
{
	HoldCurve Curve { get; }

	public HoldClipDrawer(Clip _Clip) : base(_Clip)
	{
		if (_Clip is HoldClip clip)
			Curve = clip.Curve;
	}

	protected override void DrawContent()
	{
		GUIHoldCurve.DrawSpline(ClipRect, Curve);
	}
}