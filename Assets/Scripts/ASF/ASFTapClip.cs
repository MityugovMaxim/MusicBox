using System.Collections.Generic;
using AudioBox.Compression;

namespace AudioBox.ASF
{
	public class ASFTapData
	{
		public double Time     { get; }
		public float  Position { get; }

		public ASFTapData(IDictionary<string, object> _Data)
		{
			Time     = _Data.GetDouble("time");
			Position = _Data.GetFloat("position");
		}

		public ASFTapData(double _Time, float _Position)
		{
			Time     = _Time;
			Position = _Position;
		}

		public IDictionary<string, object> Serialize()
		{
			Dictionary<string, object> data = new Dictionary<string, object>();
			data["time"]     = Time;
			data["position"] = Position;
			return data;
		}
	}

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

		public ASFTapClip(ASFTapData _Data) : this(_Data.Time, _Data.Position) { }

		public ASFTapClip(double _Time, float _Position) : base(_Time, _Time)
		{
			Position = _Position;
		}

		public override ASFClip Clone()
		{
			return new ASFTapClip(Time, Position);
		}
	}
}
