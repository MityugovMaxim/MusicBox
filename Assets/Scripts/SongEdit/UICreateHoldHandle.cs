using AudioBox.ASF;

public class UICreateHoldHandle : UICreateClipHandle<ASFHoldTrack, ASFHoldClip>
{
	protected override ASFHoldClip CreateClip(double _Time, float _Position)
	{
		const double length = 0.25d;
		
		return new ASFHoldClip(
			_Time,
			_Time + length,
			new ASFHoldKey(0, _Position),
			new ASFHoldKey(length, _Position)
		);
	}
}