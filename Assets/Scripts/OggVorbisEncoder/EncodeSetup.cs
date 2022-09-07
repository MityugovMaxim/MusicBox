using System;
using System.Collections.Generic;
using OggVorbisEncoder.Setup;
using OggVorbisEncoder.Setup.Templates;

namespace OggVorbisEncoder
{
	public class EncodeSetup
	{
		static readonly IEnumerable<ISetupTemplate> m_SetupTemplates = CreateTemplates();

		public EncodeSetup(ISetupTemplate _Template, double _BaseSetting)
		{
			Template    = _Template;
			BaseSetting = _BaseSetting;

			var iS = (int)BaseSetting;
			var ds = BaseSetting - iS;

			LowPassKilohertz = _Template.PsyLowPass[iS] * (1 - ds) + _Template.PsyLowPass[iS + 1] * ds;

			AthFloatingDecibel = _Template.PsyAthFloat[iS] * (1 - ds) + _Template.PsyAthFloat[iS + 1] * ds;

			AthAbsoluteDecibel = _Template.PsyAthAbs[iS] * (1 - ds) + _Template.PsyAthAbs[iS + 1] * ds;

			AmplitudeTrackDbPerSec = -6;

			// too low/high an ATH floater is nonsensical, but doesn't break anything 
			if (AthFloatingDecibel > -80)
				AthFloatingDecibel = -80;

			if (AthFloatingDecibel < -200)
				AthFloatingDecibel = -200;

			if (AmplitudeTrackDbPerSec > 0)
				AmplitudeTrackDbPerSec = 0;

			if (AmplitudeTrackDbPerSec < -99999)
				AmplitudeTrackDbPerSec = -99999;
		}

		public ISetupTemplate Template               { get; }
		public double         BaseSetting            { get; }
		public double         LowPassKilohertz       { get; }
		public double         AthFloatingDecibel     { get; }
		public double         AthAbsoluteDecibel     { get; }
		public double         AmplitudeTrackDbPerSec { get; }

		static IEnumerable<ISetupTemplate> CreateTemplates()
		{
			return new ISetupTemplate[]
			{
				new Stereo44SetupDataTemplate(),
				new Uncoupled44SetupDataTemplate(),
				new Stereo32SetupDataTemplate(),
				new Uncoupled32SetupDataTemplate(),
				new Stereo22SetupDataTemplate(),
				new Uncoupled22SetupDataTemplate(),
				new Stereo16SetupDataTemplate(),
				new Uncoupled16SetupDataTemplate(),
				new Stereo11SetupDataTemplate(),
				new Uncoupled11SetupDataTemplate(),
				new Stereo8SetupDataTemplate(),
				new Uncoupled8SetupDataTemplate(),
				new StereoXSetupDataTemplate(),
				new UncoupledXSetupDataTemplate(),
				new StereoXxSetupDataTemplate(),
				new UncoupledXxSetupDataTemplate()
			};
		}

		public static EncodeSetup GetBestMatch(
			int   _Channels,
			int   _SampleRate,
			float _Quality
		)
		{
			foreach (var template in m_SetupTemplates)
			{
				if (template.CouplingRestriction != -1
					&& template.CouplingRestriction != _Channels)
					continue;

				if (_SampleRate < template.SampleRateMinRestriction
					|| _SampleRate > template.SampleRateMaxRestriction)
					continue;

				var map = template.QualityMapping;

				// the template matches.  Does the requested quality mode fall within this template's modes? 
				if (_Quality < map[0]
					|| _Quality > map[template.Mappings])
					continue;

				int j;
				for (j = 0; j < template.Mappings; ++j)
					if (_Quality >= map[j] && _Quality < map[j + 1])
						break;

				// an all-points match
				double baseSetting;
				if (j == template.Mappings)
				{
					baseSetting = j - .001;
				}
				else
				{
					var low  = map[j];
					var high = map[j + 1];
					var del  = (_Quality - low) / (high - low);
					baseSetting = j + del;
				}

				return new EncodeSetup(template, baseSetting);
			}

			throw new InvalidOperationException("No matching template found");
		}
	}
}