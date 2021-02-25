[ClipDrawer(typeof(MusicClip))]
public class MusicClipDrawer : ClipDrawer
{
	MusicClip MusicClip { get; }

	public MusicClipDrawer(Clip _Clip) : base(_Clip)
	{
		MusicClip = _Clip as MusicClip;
	}
}