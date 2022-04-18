using UnityEngine;

namespace AudioBox.ASF
{
	public interface IASFColorSampler
	{
		void Sample(ASFColorClip _Source, ASFColorClip _Target, float _Phase);
	}

	public class ASFColorTrack : ASFTrack<ASFColorClip>
	{
		protected override float Size => Context.GetLocalRect().width;

		IASFColorSampler Sampler { get; }

		public ASFColorTrack(ASFTrackContext<ASFColorClip> _Context, IASFColorSampler _Sampler) : base(_Context)
		{
			Sampler = _Sampler;
		}

		public override void Sample(double _Time, double _MinTime, double _MaxTime)
		{
			(int minIndex, int maxIndex) = GetRange(_MinTime, _MaxTime);
			
			for (int i = Mathf.Max(0, minIndex); i <= maxIndex; i++)
				Clips[i].Sample(_Time);
			
			Reposition(minIndex, maxIndex, _MinTime, _MaxTime);
			
			ProcessColor(_Time);
		}

		void ProcessColor(double _Time)
		{
			(int minIndex, int maxIndex) = GetRange(_Time);
			
			if (minIndex < 0 || maxIndex < 0)
				return;
			
			ASFColorClip source = Clips[minIndex];
			ASFColorClip target = Clips[maxIndex];
			float        phase  = ASFMath.TimeToPhase(_Time, source.Time, target.Time);
			
			Sampler.Sample(source, target, phase);
		}

		(int minIndex, int maxIndex) GetRange(double _Time)
		{
			int anchor = FindAnchor(_Time);
			
			if (anchor < 0)
				return (anchor, anchor);
			
			int minIndex = FindMin(anchor, _Time);
			int maxIndex = FindMax(anchor, _Time);
			
			return (minIndex, maxIndex);
		}

		int FindAnchor(double _Time)
		{
			int i = 0;
			int j = Clips.Count - 1;
			int k = -1;
			while (i <= j)
			{
				k = (i + j) / 2;
				
				ASFColorClip clip = Clips[k];
				
				if (clip.Time < _Time)
					i = k + 1;
				else if (clip.Time > _Time)
					j = k - 1;
				else
					return k;
			}
			return k;
		}

		int FindMin(int _Anchor, double _Time)
		{
			if (_Anchor > 0 && Clips[_Anchor].Time > _Time)
			{
				int index = _Anchor - 1;
				
				ASFColorClip clip = Clips[index];
				
				if (clip.Time <= _Time)
					return index;
			}
			return _Anchor;
		}

		int FindMax(int _Anchor, double _Time)
		{
			if (_Anchor < Clips.Count - 1 && Clips[_Anchor].Time < _Time)
			{
				int index = _Anchor + 1;
				
				ASFColorClip clip = Clips[index];
				
				if (clip.Time >= _Time)
					return index;
			}
			return _Anchor;
		}

		public override ASFColorClip CreateClip()
		{
			return new ASFColorClip(0, Color.white, Color.white, Color.white, Color.white);
		}
	}
}