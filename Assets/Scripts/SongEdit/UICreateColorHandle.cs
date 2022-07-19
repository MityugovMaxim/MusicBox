using AudioBox.ASF;
using UnityEngine;

public class UICreateColorHandle : UICreateClipHandle<ASFColorTrack, ASFColorClip>
{
	protected override ASFColorClip CreateClip(double _Time, float _Position)
	{
		return new ASFColorClip(
			_Time,
			new Color(1, 1, 1),
			new Color(0f, 1f, 0.5f),
			new Color(0.23f, 0.87f, 1f),
			new Color(1f, 0.25f, 0.5f)
		);
	}
}