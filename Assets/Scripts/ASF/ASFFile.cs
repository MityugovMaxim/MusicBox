using System.Collections.Generic;
using System.Linq;
using AudioBox.Compression;
using UnityEngine.Purchasing;

namespace AudioBox.ASF
{
	public class ASFFile
	{
		public float                        BPM        { get; set; }
		public int                          Bar        { get; set; }
		public double                       Origin     { get; set; }
		public IReadOnlyList<ASFTapData>    TapData    => m_TapData;
		public IReadOnlyList<ASFDoubleData> DoubleData => m_DoubleData;
		public IReadOnlyList<ASFHoldData>   HoldData   => m_HoldData;
		public IReadOnlyList<ASFColorData>  ColorData  => m_ColorData;

		readonly List<ASFTapData>    m_TapData    = new List<ASFTapData>();
		readonly List<ASFDoubleData> m_DoubleData = new List<ASFDoubleData>();
		readonly List<ASFHoldData>   m_HoldData   = new List<ASFHoldData>();
		readonly List<ASFColorData>  m_ColorData  = new List<ASFColorData>();

		public List<T> Aggregate<T>(T _TapValue, T _DoubleValue, T _HoldValue)
		{
			SortedList<double, T> scores = new SortedList<double, T>();
			
			foreach (ASFTapData item in TapData)
				scores.Add(item.Time, _TapValue);
			
			foreach (ASFDoubleData item in DoubleData)
				scores.Add(item.Time, _DoubleValue);
			
			foreach (ASFHoldData item in HoldData)
				scores.Add(item.MinTime, _HoldValue);
			
			return scores.Values.ToList();
		}

		public IDictionary<string, object> Serialize()
		{
			Dictionary<string, object> data = new Dictionary<string, object>();
			data["bpm"]          = BPM;
			data["bar"]          = Bar;
			data["origin"]       = Origin;
			data["tap_track"]    = TapData.Select(_Clip => _Clip.Serialize()).ToList();
			data["double_track"] = DoubleData.Select(_Clip => _Clip.Serialize()).ToList();
			data["hold_track"]   = HoldData.Select(_Clip => _Clip.Serialize()).ToList();
			data["color_track"]  = ColorData.Select(_Clip => _Clip.Serialize()).ToList();
			return data;
		}

		public void Load(ASFTrack _Track)
		{
			if (_Track is ASFTapTrack tapTrack)
				LoadTap(tapTrack);
			else if (_Track is ASFDoubleTrack doubleTrack)
				LoadDouble(doubleTrack);
			else if (_Track is ASFHoldTrack holdTrack)
				LoadHold(holdTrack);
			else if (_Track is ASFColorTrack colorTrack)
				LoadColor(colorTrack);
		}

		public void Save(ASFTrack _Track)
		{
			if (_Track is ASFTapTrack tapTrack)
				SaveTap(tapTrack);
			else if (_Track is ASFDoubleTrack doubleTrack)
				SaveDouble(doubleTrack);
			else if (_Track is ASFHoldTrack holdTrack)
				SaveHold(holdTrack);
			else if (_Track is ASFColorTrack colorTrack)
				SaveColor(colorTrack);
		}

		public void LoadTap(ASFTapTrack _Track)
		{
			foreach (ASFTapData data in TapData)
				_Track.AddClip(new ASFTapClip(data));
		}

		public void SaveTap(ASFTapTrack _Track)
		{
			m_TapData.Clear();
			foreach (ASFTapClip clip in _Track.Clips)
				m_TapData.Add(new ASFTapData(clip.Time, clip.Position));
		}

		public void LoadDouble(ASFDoubleTrack _Track)
		{
			foreach (ASFDoubleData data in DoubleData)
				_Track.AddClip(new ASFDoubleClip(data));
		}

		public void SaveDouble(ASFDoubleTrack _Track)
		{
			m_DoubleData.Clear();
			foreach (ASFDoubleClip clip in _Track.Clips)
				m_DoubleData.Add(new ASFDoubleData(clip.Time));
		}

		public void LoadHold(ASFHoldTrack _Track)
		{
			foreach (ASFHoldData data in HoldData)
				_Track.AddClip(new ASFHoldClip(data));
		}

		public void SaveHold(ASFHoldTrack _Track)
		{
			m_HoldData.Clear();
			foreach (ASFHoldClip clip in _Track.Clips)
				m_HoldData.Add(new ASFHoldData(clip.MinTime, clip.MaxTime, clip.Keys));
		}

		public void LoadColor(ASFColorTrack _Track)
		{
			foreach (ASFColorData data in ColorData)
				_Track.AddClip(new ASFColorClip(data));
		}

		public void SaveColor(ASFColorTrack _Track)
		{
			m_ColorData.Clear();
			foreach (ASFColorClip clip in _Track.Clips)
				m_ColorData.Add(new ASFColorData(clip.Time, clip.BackgroundPrimary, clip.BackgroundSecondary, clip.ForegroundPrimary, clip.ForegroundSecondary));
		}

		public ASFFile(float _BPM, int _Bar, double _Origin)
		{
			BPM    = _BPM;
			Bar    = _Bar;
			Origin = _Origin;
		}

		public ASFFile(string _Data) : this(MiniJson.JsonDecode(_Data) as IDictionary<string, object>) { }

		public ASFFile(IDictionary<string, object> _Data)
		{
			BPM    = _Data.GetFloat("bpm");
			Bar    = _Data.GetInt("bar");
			Origin = _Data.GetDouble("origin");
			
			m_TapData = _Data.GetList("tap")
				.OfType<IDictionary<string, object>>()
				.Select(_Clip => new ASFTapData(_Clip))
				.ToList();
			
			m_DoubleData = _Data.GetList("double")
				.OfType<IDictionary<string, object>>()
				.Select(_Clip => new ASFDoubleData(_Clip))
				.ToList();
			
			m_HoldData = _Data.GetList("hold")
				.OfType<IDictionary<string, object>>()
				.Select(_Clip => new ASFHoldData(_Clip))
				.ToList();
			
			m_ColorData = _Data.GetList("color")
				.OfType<IDictionary<string, object>>()
				.Select(_Clip => new ASFColorData(_Clip))
				.ToList();
		}
	}
}
