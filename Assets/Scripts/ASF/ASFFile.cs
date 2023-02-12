using System;
using System.Collections.Generic;
using System.Linq;
using AudioBox.Compression;

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
			data["bpm"]    = BPM;
			data["bar"]    = Bar;
			data["origin"] = Origin;
			data["tap"]    = TapData.Select(_Clip => _Clip.Serialize()).ToList();
			data["double"] = DoubleData.Select(_Clip => _Clip.Serialize()).ToList();
			data["hold"]   = HoldData.Select(_Clip => _Clip.Serialize()).ToList();
			data["color"]  = ColorData.Select(_Clip => _Clip.Serialize()).ToList();
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

		public void Trim(double _Time)
		{
			for (int i = m_TapData.Count - 1; i >= 0; i--)
			{
				ASFTapData data = m_TapData[i];
				
				if (data == null || data.Time < _Time)
					continue;
				
				m_TapData.RemoveAt(i);
			}
			
			for (int i = m_DoubleData.Count - 1; i >= 0; i--)
			{
				ASFDoubleData data = m_DoubleData[i];
				
				if (data == null || data.Time < _Time)
					continue;
				
				m_DoubleData.RemoveAt(i);
			}
			
			for (int i = 0; i < m_HoldData.Count; i++)
			{
				ASFHoldData data = m_HoldData[i];
				
				if (data == null || data.MaxTime < _Time)
					continue;
				
				m_HoldData.RemoveAt(i);
			}
			
			for (int i = 0; i < m_ColorData.Count; i++)
			{
				ASFColorData data = m_ColorData[i];
				
				if (data == null || data.Time < _Time)
					continue;
				
				m_ColorData.RemoveAt(i);
			}
		}

		public void LoadTap(ASFTapTrack _Track)
		{
			_Track.ClearClips();
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
			_Track.ClearClips();
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
			_Track.ClearClips();
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
			_Track.ClearClips();
			foreach (ASFColorData data in ColorData)
				_Track.AddClip(new ASFColorClip(data));
		}

		public void SaveColor(ASFColorTrack _Track)
		{
			m_ColorData.Clear();
			foreach (ASFColorClip clip in _Track.Clips)
				m_ColorData.Add(new ASFColorData(clip.Time, clip.BackgroundPrimary, clip.BackgroundSecondary, clip.ForegroundPrimary, clip.ForegroundSecondary));
		}

		public ASFFile(IDictionary<string, object> _Data)
		{
			BPM    = _Data.GetFloat("bpm");
			Bar    = _Data.GetInt("bar");
			Origin = _Data.GetDouble("origin");
			
			ParseTap(_Data);
			ParseDouble(_Data);
			ParseHold(_Data);
			ParseColor(_Data);
		}

		void ParseTap(IDictionary<string, object> _Data) => ParseClips(
			_Data,
			"tap",
			m_TapData,
			_Entry => new ASFTapData(_Entry)
		);

		void ParseDouble(IDictionary<string, object> _Data) => ParseClips(
			_Data,
			"double",
			m_DoubleData,
			_Entry => new ASFDoubleData(_Entry)
		);

		void ParseHold(IDictionary<string, object> _Data) => ParseClips(
			_Data,
			"hold",
			m_HoldData,
			_Entry => new ASFHoldData(_Entry)
		);

		void ParseColor(IDictionary<string, object> _Data) => ParseClips(
			_Data,
			"color",
			m_ColorData,
			_Entry => new ASFColorData(_Entry)
		);

		static void ParseClips<T>(IDictionary<string, object> _Data, string _Key, List<T> _Collection, Func<IDictionary<string, object>, T> _Selector)
		{
			if (_Collection == null)
				return;
			
			_Collection.Clear();
			
			if (string.IsNullOrEmpty(_Key))
				return;
			
			IList<object> data = _Data.GetList(_Key);
			
			if (data == null || data.Count == 0)
				return;
			
			IList<T> clips = data
				.OfType<IDictionary<string, object>>()
				.Select(_Selector)
				.ToList();
			
			_Collection.AddRange(clips);
		}
	}
}
