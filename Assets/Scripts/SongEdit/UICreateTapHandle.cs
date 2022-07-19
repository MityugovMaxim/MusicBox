using AudioBox.ASF;

public class UICreateTapHandle : UICreateClipHandle<ASFTapTrack, ASFTapClip>
{
	protected override ASFTapClip CreateClip(double _Time, float _Position)
	{
		return new ASFTapClip(_Time, _Position);
	}
}