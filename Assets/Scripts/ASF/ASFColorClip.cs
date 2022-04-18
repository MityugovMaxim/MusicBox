using System.Collections.Generic;
using AudioBox.Compression;
using UnityEngine;

namespace AudioBox.ASF
{
	public class ASFColorClip : ASFClip
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
		public Color BackgroundPrimary   { get; set; }
		public Color BackgroundSecondary { get; set; }
		public Color ForegroundPrimary   { get; set; }
		public Color ForegroundSecondary { get; set; }

		public ASFColorClip(
			double _Time,
			Color  _BackgroundPrimary,
			Color  _BackgroundSecondary,
			Color  _ForegroundPrimary,
			Color  _ForegroundSecondary
		) : base(_Time, _Time)
		{
			BackgroundPrimary   = _BackgroundPrimary;
			BackgroundSecondary = _BackgroundSecondary;
			ForegroundPrimary   = _ForegroundPrimary;
			ForegroundSecondary = _ForegroundSecondary;
		}

		public override ASFClip Clone()
		{
			return new ASFColorClip(Time, BackgroundPrimary, BackgroundSecondary, ForegroundPrimary, ForegroundSecondary);
		}

		public override object Serialize()
		{
			IDictionary<string, object> data = new Dictionary<string, object>();
			
			data["time"]                 = Time;
			data["background_primary"]   = '#' + ColorUtility.ToHtmlStringRGBA(BackgroundPrimary);
			data["background_secondary"] = '#' + ColorUtility.ToHtmlStringRGBA(BackgroundSecondary);
			data["foreground_primary"]   = '#' + ColorUtility.ToHtmlStringRGBA(ForegroundPrimary);
			data["foreground_secondary"] = '#' + ColorUtility.ToHtmlStringRGBA(ForegroundSecondary);
			
			return data;
		}

		public override void Deserialize(IDictionary<string, object> _Data)
		{
			if (_Data == null)
				return;
			
			Time                = _Data.GetDouble("time");
			BackgroundPrimary   = _Data.GetHtmlColor("background_primary");
			BackgroundSecondary = _Data.GetHtmlColor("background_secondary");
			ForegroundPrimary   = _Data.GetHtmlColor("foreground_primary");
			ForegroundSecondary = _Data.GetHtmlColor("foreground_secondary");
		}
	}
}