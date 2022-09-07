using System;

namespace OggVorbisEncoder.Setup
{
	public class CodeBook
	{
		public CodeBook(
			int             _Dimensions,
			int             _Entries,
			int             _UsedEntries,
			IStaticCodeBook _StaticBook,
			float[]         _ValueList,
			uint[]          _CodeList,
			int[]           _DecIndex,
			byte[]          _DecCodeLengths,
			uint[]          _DecFirstTable,
			int             _DecFirstTableN,
			int             _DecMaxLength,
			int             _QuantValues,
			int             _MinVal,
			int             _Delta
		)
		{
			Dimensions     = _Dimensions;
			Entries        = _Entries;
			UsedEntries    = _UsedEntries;
			StaticBook     = _StaticBook;
			ValueList      = _ValueList;
			CodeList       = _CodeList;
			DecIndex       = _DecIndex;
			DecCodeLengths = _DecCodeLengths;
			DecFirstTable  = _DecFirstTable;
			DecFirstTableN = _DecFirstTableN;
			DecMaxLength   = _DecMaxLength;
			QuantValues    = _QuantValues;
			MinVal         = _MinVal;
			Delta          = _Delta;
		}

		/// <summary>
		///     codebook dimensions (elements per vector)
		/// </summary>
		public int Dimensions { get; }

		/// <summary>
		///     codebook entries
		/// </summary>
		public int Entries { get; }

		/// <summary>
		///     populated codebook entries
		/// </summary>
		public int UsedEntries { get; }

		public IStaticCodeBook StaticBook { get; }

		/* for encode, the below are entry-ordered, fully populated */
		/* for decode, the below are ordered by bitreversed codeword and only
			used entries are populated */

		/// <summary>
		///     list of dim*entries actual entry values
		/// </summary>
		public float[] ValueList { get; }

		/// <summary>
		///     list of bitstream codewords for each entry
		/// </summary>
		public uint[] CodeList { get; }

		/// <summary>
		///     only used if sparseness collapsed
		/// </summary>
		public int[] DecIndex { get; }

		public byte[] DecCodeLengths { get; }


		public uint[] DecFirstTable { get; }

		public int DecFirstTableN { get; }

		public int DecMaxLength { get; }

		/* The current encoder uses only centered, integer-only lattice books. */
		public int QuantValues { get; }
		public int MinVal      { get; }
		public int Delta       { get; }

		public static CodeBook InitEncode(IStaticCodeBook _Source)
		{
			return new CodeBook(
				_Source.Dimensions,
				_Source.LengthList.Length,
				_Source.LengthList.Length,
				_Source,
				null,
				Encoding.MakeWords(_Source.LengthList, 0),
				null,
				null,
				null,
				0,
				0,
				_Source.GetQuantVals(),
				(int)Math.Round(Encoding.UnpackFloat(_Source.QuantMin)),
				(int)Math.Round(Encoding.UnpackFloat(_Source.QuantDelta))
			);
		}
	}
}