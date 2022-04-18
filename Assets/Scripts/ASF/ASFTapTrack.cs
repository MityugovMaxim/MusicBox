using UnityEngine;

namespace AudioBox.ASF
{
	public class ASFTapTrack : ASFTrack<ASFTapClip>
	{
		const int COUNT = 4;

		protected override float Size => Context.GetLocalRect().width / COUNT;

		public ASFTapTrack(ASFTrackContext<ASFTapClip> _Context) : base(_Context) { }

		public override void Sample(double _Time, double _MinTime, double _MaxTime)
		{
			(int minIndex, int maxIndex) = GetRange(_MinTime, _MaxTime);
			
			for (int i = Mathf.Max(0, minIndex); i <= maxIndex; i++)
				Clips[i].Sample(_Time);
			
			Reposition(minIndex, maxIndex, _MinTime, _MaxTime);
		}

		public override ASFTapClip CreateClip()
		{
			return new ASFTapClip(0, 0);
		}

		protected override Rect GetViewRect(ASFTapClip _Clip, Rect _Rect, double _MinTime, double _MaxTime, float _Padding = 0)
		{
			Rect rect = base.GetViewRect(_Clip, _Rect, _MinTime, _MaxTime, _Padding);
			
			float width = _Rect.width / COUNT;
			
			return new Rect(
				rect.x + (_Rect.width - width) * _Clip.Position,
				rect.y,
				width,
				rect.height
			);
		}
	}
}