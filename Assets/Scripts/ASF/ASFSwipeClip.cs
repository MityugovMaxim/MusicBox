using System.Collections.Generic;
using AudioBox.Compression;

namespace AudioBox.ASF
{
	public class ASFSwipeClip : ASFClip
	{
		public enum Direction
		{
			Left,
			Right,
			Up,
			Down,
		}

		public double Time
		{
			get => MinTime;
			set
			{
				MinTime = value;
				MaxTime = value;
			}
		}

		public Direction Type { get; set; }

		public ASFSwipeClip(double _Time, Direction _Type) : base(_Time, _Time)
		{
			Type = _Type;
		}

		public override ASFClip Clone()
		{
			return new ASFSwipeClip(Time, Type);
		}

		public override object Serialize()
		{
			IDictionary<string, object> data = new Dictionary<string, object>();
			
			data["time"] = MinTime;
			data["type"] = (int)Type;
			
			return data;
		}

		public override void Deserialize(IDictionary<string, object> _Data)
		{
			if (_Data == null)
				return;
			
			Time = _Data.GetDouble("time");
			Type = _Data.GetEnum<Direction>("type");
		}
	}
}