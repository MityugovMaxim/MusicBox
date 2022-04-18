using System.Collections.Generic;
using AudioBox.Compression;

namespace AudioBox.ASF
{
	public class ASFTapClip : ASFClip
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

		public float Position { get; set; }

		public ASFTapClip(double _Time, float _Position) : base(_Time, _Time)
		{
			Position = _Position;
		}

		public override ASFClip Clone()
		{
			return new ASFTapClip(Time, Position);
		}

		public override object Serialize()
		{
			IDictionary<string, object> data = new Dictionary<string, object>();
			
			data["time"]     = Time;
			data["position"] = Position;
			
			return data;
		}

		public override void Deserialize(IDictionary<string, object> _Data)
		{
			if (_Data == null)
				return;
			
			Time     = _Data.GetDouble("time");
			Position = _Data.GetFloat("position");
		}
	}
}