using OggVorbisEncoder.Lookups;
using OggVorbisEncoder.Setup;

namespace OggVorbisEncoder
{
	public class LookupCollection
	{
		LookupCollection(
			EnvelopeLookup  _EnvelopeLookup,
			MdctLookup[]    _TransformLookup,
			PsyGlobalLookup _PsyGlobalLookup,
			PsyLookup[]     _PsyLookup,
			DrftLookup[]    _FFTLookup,
			FloorLookup[]   _FloorLookup,
			ResidueLookup[] _ResidueLookup
		)
		{
			EnvelopeLookup  = _EnvelopeLookup;
			TransformLookup = _TransformLookup;
			PsyGlobalLookup = _PsyGlobalLookup;
			PsyLookup       = _PsyLookup;
			FftLookup       = _FFTLookup;
			FloorLookup     = _FloorLookup;
			ResidueLookup   = _ResidueLookup;
		}

		public EnvelopeLookup  EnvelopeLookup  { get; }
		public MdctLookup[]    TransformLookup { get; }
		public PsyGlobalLookup PsyGlobalLookup { get; }
		public PsyLookup[]     PsyLookup       { get; }
		public DrftLookup[]    FftLookup       { get; }
		public FloorLookup[]   FloorLookup     { get; }
		public ResidueLookup[] ResidueLookup   { get; }

		public static LookupCollection Create(VorbisInfo _VorbisInfo)
		{
			var codecSetup = _VorbisInfo.CodecSetup;

			var psyGlobal = new PsyGlobalLookup(codecSetup.PsyGlobalParam);
			var envelope  = new EnvelopeLookup(codecSetup.PsyGlobalParam, _VorbisInfo);

			// MDCT is tranform 0
			var transform = new MdctLookup[2];
			transform[0] = new MdctLookup(codecSetup.BlockSizes[0]);
			transform[1] = new MdctLookup(codecSetup.BlockSizes[1]);

			// analysis always needs an fft
			var fftLookup = new DrftLookup[2];
			fftLookup[0] = new DrftLookup(codecSetup.BlockSizes[0]);
			fftLookup[1] = new DrftLookup(codecSetup.BlockSizes[1]);

			// finish the codebooks 
			if (codecSetup.FullBooks == null)
			{
				codecSetup.FullBooks = new CodeBook[codecSetup.BookParams.Count];
				for (var i = 0; i < codecSetup.BookParams.Count; i++)
					codecSetup.FullBooks[i] = CodeBook.InitEncode(codecSetup.BookParams[i]);
			}

			var psyLookup = new PsyLookup[codecSetup.PsyParams.Count];
			for (var i = 0; i < psyLookup.Length; i++)
				psyLookup[i] = new PsyLookup(
					codecSetup.PsyParams[i],
					codecSetup.PsyGlobalParam,
					codecSetup.BlockSizes[codecSetup.PsyParams[i].BlockFlag] / 2,
					_VorbisInfo.SampleRate
				);

			// initialize all the backend lookups 
			var floor = new FloorLookup[codecSetup.FloorParams.Count];
			for (var i = 0; i < floor.Length; i++)
				floor[i] = new FloorLookup(codecSetup.FloorParams[i]);

			var residue = new ResidueLookup[codecSetup.ResidueParams.Count];
			for (var i = 0; i < residue.Length; i++)
				residue[i] = new ResidueLookup(codecSetup.ResidueParams[i], codecSetup.FullBooks);

			return new LookupCollection(
				envelope,
				transform,
				psyGlobal,
				psyLookup,
				fftLookup,
				floor,
				residue
			);
		}
	}
}