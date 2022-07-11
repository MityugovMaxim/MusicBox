using System.Collections;
using System.Collections.Generic;
using AudioBox.Compression;
using UnityEngine;

namespace AudioBox.ASF
{
	public abstract class ASFTrack
	{
		public abstract void Sample(double _Time, double _MinTime, double _MaxTime);

		public abstract object Serialize();

		public abstract void Deserialize(IList<object> _Data);
	}

	public abstract class ASFTrack<T> : ASFTrack where T : ASFClip
	{
		public IReadOnlyList<T> Clips => m_Clips;

		public ASFTrackContext<T> Context { get; }

		protected abstract float Size { get; }

		int  MinIndex { get; set; } = -1;
		int  MaxIndex { get; set; } = -1;
		Rect Rect     { get; }

		readonly List<T> m_Clips = new List<T>();

		public ASFTrack(ASFTrackContext<T> _Context)
		{
			Context = _Context;
			
			Rect = Context.GetLocalRect();
			
			Context.Clear();
		}

		public override object Serialize()
		{
			IList data = new List<object>();
			
			foreach (T clip in Clips)
			{
				if (clip != null)
					data.Add(clip.Serialize());
			}
			
			return data;
		}

		public override void Deserialize(IList<object> _Data)
		{
			MinIndex = -1;
			MaxIndex = -1;
			
			Context.Clear();
			m_Clips.Clear();
			
			if (_Data == null)
				return;
			
			for (int i = 0; i < _Data.Count; i++)
			{
				T clip = CreateClip();
				
				if (clip == null)
					continue;
				
				clip.Deserialize(_Data.GetDictionary(i));
				
				AddClip(clip);
			}
		}

		public void AddClip(T _Clip)
		{
			MinIndex = -1;
			MaxIndex = -1;
			
			Context.Clear();
			
			m_Clips.Add(_Clip);
			
			SortClips();
		}

		public void RemoveClip(T _Clip)
		{
			MinIndex = -1;
			MaxIndex = -1;
			
			Context.Clear();
			
			m_Clips.Remove(_Clip);
			
			SortClips();
		}

		public void SortClips()
		{
			m_Clips.Sort((_A, _B) => _A.MinTime.CompareTo(_B.MinTime));
		}

		public abstract T CreateClip();

		protected (int minIndex, int maxIndex) GetRange(double _MinTime, double _MaxTime)
		{
			float padding = Size * 0.5f;
			
			double minTime = ASFMath.PositionToTime(Rect.yMin - padding, Rect.yMin, Rect.yMax, _MinTime, _MaxTime);
			double maxTime = ASFMath.PositionToTime(Rect.yMax + padding, Rect.yMin, Rect.yMax, _MinTime, _MaxTime);
			
			int anchor = FindAnchor(minTime, maxTime);
			
			if (anchor < 0)
				return (anchor, anchor);
			
			int minIndex = FindMin(anchor, minTime);
			int maxIndex = FindMax(anchor, maxTime);
			
			return (minIndex, maxIndex);
		}

		protected virtual void Reposition(int _MinIndex, int _MaxIndex, double _MinTime, double _MaxTime)
		{
			float padding = Size * 0.5f;
			
			// Remove clips
			for (int i = Mathf.Max(0, MinIndex); i <= MaxIndex; i++)
			{
				T clip = Clips[i];
				
				if (clip == null || i >= _MinIndex && i <= _MaxIndex)
					continue;
				
				Rect clipRect = GetClipRect(clip, Rect, _MinTime, _MaxTime);
				Rect viewRect = GetViewRect(clip, Rect, _MinTime, _MaxTime, padding);
				
				Context.RemoveClip(clip, clipRect, viewRect);
			}
			
			// Add clips
			for (int i = Mathf.Max(0, _MinIndex); i <= _MaxIndex; i++)
			{
				T clip = Clips[i];
				
				if (clip == null || i >= MinIndex && i <= MaxIndex)
					continue;
				
				Rect clipRect = GetClipRect(clip, Rect, _MinTime, _MaxTime);
				Rect viewRect = GetViewRect(clip, Rect, _MinTime, _MaxTime, padding);
				
				Context.AddClip(clip, clipRect, viewRect);
			}
			
			// Process clips
			for (int i = Mathf.Max(0, _MinIndex); i <= _MaxIndex; i++)
			{
				T clip = Clips[i];
				
				if (clip == null || i < MinIndex || i > MaxIndex)
					continue;
				
				Rect clipRect = GetClipRect(clip, Rect, _MinTime, _MaxTime);
				Rect viewRect = GetViewRect(clip, Rect, _MinTime, _MaxTime, padding);
				
				Context.ProcessClip(clip, clipRect, viewRect);
			}
			
			MinIndex = _MinIndex;
			MaxIndex = _MaxIndex;
		}

		protected virtual Rect GetClipRect(T _Clip, Rect _Rect, double _MinTime, double _MaxTime)
		{
			float minPosition = ASFMath.TimeToPosition(_Clip.MinTime, _MinTime, _MaxTime, _Rect.yMin, _Rect.yMax);
			float maxPosition = ASFMath.TimeToPosition(_Clip.MaxTime, _MinTime, _MaxTime, _Rect.yMin, _Rect.yMax);
			
			return new Rect(
				_Rect.x,
				minPosition,
				_Rect.width,
				maxPosition - minPosition
			);
		}

		protected virtual Rect GetViewRect(T _Clip, Rect _Rect, double _MinTime, double _MaxTime, float _Padding = 0)
		{
			float minPosition = ASFMath.TimeToPosition(_Clip.MinTime, _MinTime, _MaxTime, _Rect.yMin, _Rect.yMax) - _Padding;
			float maxPosition = ASFMath.TimeToPosition(_Clip.MaxTime, _MinTime, _MaxTime, _Rect.yMin, _Rect.yMax) + _Padding;
			
			return new Rect(
				_Rect.x,
				minPosition,
				_Rect.width,
				maxPosition - minPosition
			);
		}

		int FindMin(int _Anchor, double _MinTime)
		{
			int index = _Anchor;
			while (index > 0)
			{
				ASFClip clip = Clips[index - 1];
				
				if (clip.MaxTime < _MinTime)
					break;
				
				index--;
			}
			return index;
		}

		int FindMax(int _Anchor, double _MaxTime)
		{
			int index = _Anchor;
			while (index < Clips.Count - 1)
			{
				ASFClip clip = Clips[index + 1];
				
				if (clip.MinTime > _MaxTime)
					break;
				
				index++;
			}
			return index;
		}

		int FindAnchor(double _MinTime, double _MaxTime)
		{
			int i = 0;
			int j = Clips.Count - 1;
			while (i <= j)
			{
				int k = (i + j) / 2;
				
				ASFClip clip = Clips[k];
				
				if (clip.MaxTime < _MinTime)
					i = k + 1;
				else if (clip.MinTime > _MaxTime)
					j = k - 1;
				else
					return k;
			}
			return -1;
		}
	}
}