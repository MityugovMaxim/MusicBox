using UnityEngine;

namespace AudioBox.ASF
{
	public abstract class ASFTrackContext<TClip> : UIEntity where TClip : ASFClip
	{
		public abstract void AddClip(TClip _Clip, Rect _ClipRect, Rect _ViewRect);

		public abstract void RemoveClip(TClip _Clip, Rect _ClipRect, Rect _ViewRect);

		public abstract void ProcessClip(TClip _Clip, Rect _ClipRect, Rect _ViewRect);

		public abstract void Clear();
	}
}