using System.Collections.Generic;
using AudioBox.Compression;

namespace AudioBox.ASF
{
	public class ASFHoldKey
	{
		public double Time     { get; set; }
		public float  Position { get; set; }

		public ASFHoldKey() { }

		public ASFHoldKey(IDictionary<string, object> _Data)
		{
			Time     = _Data.GetDouble("time");
			Position = _Data.GetFloat("position");
		}

		public ASFHoldKey(double _Time, float _Position)
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
}