using System.Collections.Generic;
using AudioBox.Compression;

namespace AudioBox.ASF
{
	public class ASFDoubleData
	{
		public double Time { get; }

		public ASFDoubleData(IDictionary<string, object> _Data)
		{
			Time = _Data.GetDouble("time");
		}

		public ASFDoubleData(double _Time)
		{
			Time = _Time;
		}

		public IDictionary<string, object> Serialize()
		{
			Dictionary<string, object> data = new Dictionary<string, object>();
			data["time"] = Time;
			return data;
		}
	}

	public class ASFDoubleClip : ASFClip
	{
		public double Time
		{
			get => MinTime;
			set
			{
				MinTime = value;
				MaxTime = value;
			}
		}

		public ASFDoubleClip(ASFDoubleData _Data) : this(_Data.Time) { }

		public ASFDoubleClip(double _Time) : base(_Time, _Time) { }

		public override ASFClip Clone()
		{
			return new ASFDoubleClip(Time);
		}
	}
}
