using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AudioBox.Compression;

namespace AudioBox.ASF
{
	public class ASFHoldData
	{
		public double                    MinTime { get; }
		public double                    MaxTime { get; }
		public IReadOnlyList<ASFHoldKey> Keys    { get; }

		public ASFHoldData(IDictionary<string, object> _Data)
		{
			MinTime = _Data.GetDouble("min_time");
			MaxTime = _Data.GetDouble("max_time");
			Keys = _Data.GetList("keys")
				.OfType<IDictionary<string, object>>()
				.Select(_Key => new ASFHoldKey(_Key))
				.ToList();
		}

		public ASFHoldData(double _MinTime, double _MaxTime, List<ASFHoldKey> _Keys)
		{
			MinTime = _MinTime;
			MaxTime = _MaxTime;
			Keys    = _Keys;
		}

		public IDictionary<string, object> Serialize()
		{
			Dictionary<string, object> data = new Dictionary<string, object>();
			data["min_time"] = MinTime;
			data["max_time"] = MaxTime;
			data["keys"]     = Keys.Select(_Key => _Key.Serialize()).ToList();
			return data;
		}
	}

	public class ASFHoldClip : ASFClip
	{
		public float Phase { get; private set; }

		public List<ASFHoldKey> Keys { get; }

		public ASFHoldClip(ASFHoldData _Data) : base(_Data.MinTime, _Data.MaxTime)
		{
			Keys = new List<ASFHoldKey>(_Data.Keys);
		}

		public ASFHoldClip(double _MinTime, double _MaxTime) : base(_MinTime, _MaxTime)
		{
			Keys = new List<ASFHoldKey>()
			{
				new ASFHoldKey(_MinTime, 0.5f),
				new ASFHoldKey(_MaxTime, 0.5f),
			};
		}

		public ASFHoldClip(double _MinTime, double _MaxTime, params ASFHoldKey[] _Keys) : base(_MinTime, _MaxTime)
		{
			Keys = new List<ASFHoldKey>(_Keys);
		}

		public override ASFClip Clone()
		{
			ASFHoldKey[] keys = new ASFHoldKey[Keys.Count];
			
			for (int i = 0; i < Keys.Count; i++)
				keys[i] = new ASFHoldKey(Keys[i].Time, Keys[i].Position);
			
			return new ASFHoldClip(MinTime, MaxTime, keys);
		}

		protected override void OnEnterMin(double _Time)
		{
			Phase = 0;
		}

		protected override void OnEnterMax(double _Time)
		{
			Phase = 1;
		}

		protected override void OnSample(double _Time)
		{
			Phase = ASFMath.TimeToPhase(_Time, MinTime, MaxTime);
		}

		protected override void OnExitMin(double _Time)
		{
			Phase = 0;
		}

		protected override void OnExitMax(double _Time)
		{
			Phase = 1;
		}
	}
}
