using System.Collections;
using System.Collections.Generic;
using AudioBox.Compression;

namespace AudioBox.ASF
{
	public class ASFHoldClip : ASFClip
	{
		public class Key
		{
			public double Time     { get; set; }
			public float  Position { get; set; }

			public Key() { }

			public Key(double _Time, float _Position)
			{
				Time     = _Time;
				Position = _Position;
			}
		}

		public float Phase { get; private set; }

		public List<Key> Keys { get; }

		public ASFHoldClip(double _MinTime, double _MaxTime) : base(_MinTime, _MaxTime)
		{
			Keys = new List<Key>()
			{
				new Key(_MinTime, 0.5f),
				new Key(_MaxTime, 0.5f),
			};
		}

		public ASFHoldClip(double _MinTime, double _MaxTime, params Key[] _Keys) : base(_MinTime, _MaxTime)
		{
			Keys = new List<Key>(_Keys);
		}

		public override ASFClip Clone()
		{
			Key[] keys = new Key[Keys.Count];
			
			for (int i = 0; i < Keys.Count; i++)
				keys[i] = new Key(Keys[i].Time, Keys[i].Position);
			
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

		public override object Serialize()
		{
			IDictionary<string, object> data = new Dictionary<string, object>();
			
			data["min_time"] = MinTime;
			data["max_time"] = MaxTime;
			
			IList keysData = new List<object>();
			
			foreach (Key key in Keys)
			{
				if (key == null)
					continue;
				
				IDictionary<string, object> keyData = new Dictionary<string, object>();
				
				keyData["time"]     = key.Time;
				keyData["position"] = key.Position;
				
				keysData.Add(keyData);
			}
			
			data["keys"] = keysData;
			
			return data;
		}

		public override void Deserialize(IDictionary<string, object> _Data)
		{
			if (_Data == null)
				return;
			
			MinTime = _Data.GetDouble("min_time");
			MaxTime = _Data.GetDouble("max_time");
			
			Keys.Clear();
			
			IList<object> keysData = _Data.GetList("keys");
			for (int i = 0; i < keysData.Count; i++)
			{
				IDictionary<string, object> keyData = keysData.GetDictionary(i);
				
				if (keyData == null)
					continue;
				
				Key key = new Key(
					keyData.GetDouble("time"),
					keyData.GetFloat("position")
				);
				
				Keys.Add(key);
			}
		}
	}
}