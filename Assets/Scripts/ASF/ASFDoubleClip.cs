using System.Collections.Generic;
using AudioBox.Compression;

namespace AudioBox.ASF
{
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

		public ASFDoubleClip(double _Time) : base(_Time, _Time) { }

		public override ASFClip Clone()
		{
			return new ASFDoubleClip(Time);
		}

		public override object Serialize()
		{
			IDictionary<string, object> data = new Dictionary<string, object>();
			
			data["time"] = MinTime;
			
			return data;
		}

		public override void Deserialize(IDictionary<string, object> _Data)
		{
			if (_Data == null)
				return;
			
			Time = _Data.GetDouble("time");
		}
	}
}