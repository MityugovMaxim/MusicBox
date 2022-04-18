using UnityEngine;

namespace AudioBox.ASF
{
	public abstract class ASFClipContext<TClip> : UIEntity where TClip : ASFClip
	{
		public TClip Clip { get; private set; }

		public Rect ViewRect
		{
			get => new Rect(RectTransform.anchoredPosition - Vector2.Scale(RectTransform.sizeDelta, RectTransform.pivot), RectTransform.sizeDelta);
			set
			{
				RectTransform.anchoredPosition = value.center;
				RectTransform.sizeDelta        = value.size;
			}
		}

		public Rect ClipRect { get; set; }

		protected RectTransform Container { get; private set; }

		protected virtual void Setup(RectTransform _Container, TClip _Clip, Rect _ClipRect, Rect _ViewRect)
		{
			Vector2 pivot = _Container.pivot;
			RectTransform.SetParent(_Container);
			RectTransform.anchorMin = pivot;
			RectTransform.anchorMax = pivot;
			RectTransform.pivot     = new Vector2(0.5f, 0.5f);
			
			Container = _Container;
			Clip      = _Clip;
			ClipRect  = _ClipRect;
			ViewRect  = _ViewRect;
		}
	}
}