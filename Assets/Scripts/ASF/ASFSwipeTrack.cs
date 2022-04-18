using UnityEngine;

namespace AudioBox.ASF
{
	public class ASFSwipeTrack : ASFTrack<ASFSwipeClip>
	{
		const int COUNT = 4;

		protected override float Size => Context.GetLocalRect().width / COUNT;

		public ASFSwipeTrack(ASFTrackContext<ASFSwipeClip> _Context) : base(_Context) { }

		public override void Sample(double _Time, double _MinTime, double _MaxTime)
		{
			(int minIndex, int maxIndex) = GetRange(_MinTime, _MaxTime);
			
			for (int i = Mathf.Max(0, minIndex); i <= maxIndex; i++)
				Clips[i].Sample(_Time);
			
			Reposition(minIndex, maxIndex, _MinTime, _MaxTime);
		}

		public override ASFSwipeClip CreateClip()
		{
			return new ASFSwipeClip(0, ASFSwipeClip.Direction.Up);
		}
	}
}