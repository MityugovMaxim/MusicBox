namespace AudioBox.ASF
{
	public class ASFDoubleTrack : ASFTrack<ASFDoubleClip>
	{
		const int COUNT = 4;

		protected override float Size { get; }

		public ASFDoubleTrack(ASFTrackContext<ASFDoubleClip> _Context) : base(_Context)
		{
			Size = Context.GetLocalRect().width / COUNT;
		}

		public override void Sample(double _Time, double _MinTime, double _MaxTime)
		{
			(int minIndex, int maxIndex) = GetRange(_MinTime, _MaxTime);
			
			Reposition(minIndex, maxIndex, _MinTime, _MaxTime);
		}

		public override ASFDoubleClip CreateClip()
		{
			return new ASFDoubleClip(0);
		}
	}
}