using AudioBox.ASF;

public class UICreateDoubleHandle : UICreateClipHandle<ASFDoubleTrack, ASFDoubleClip>
{
	protected override ASFDoubleClip CreateClip(double _Time, float _Position)
	{
		return new ASFDoubleClip(_Time);
	}
}