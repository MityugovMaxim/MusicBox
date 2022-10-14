namespace OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo16.Uncoupled.Chapter2
{
	public class Page40 : IStaticCodeBook
	{
		public int Dimensions { get; } = 2;

		public byte[] LengthList { get; } =
		{
			2, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 11,
			11, 5, 5, 5, 7, 6, 8, 7, 9, 9, 9, 9, 10, 10, 11, 11,
			12, 12, 5, 5, 5, 6, 6, 7, 8, 8, 9, 9, 9, 10, 10, 11,
			11, 12, 12, 6, 7, 6, 7, 7, 8, 8, 9, 9, 9, 9, 10, 10,
			11, 11, 12, 12, 6, 6, 7, 7, 7, 8, 8, 9, 9, 9, 9, 10,
			10, 11, 11, 12, 12, 7, 8, 8, 8, 8, 9, 9, 9, 9, 10, 10,
			11, 11, 11, 11, 12, 12, 7, 7, 8, 8, 8, 9, 9, 9, 9, 10,
			10, 11, 11, 11, 11, 12, 12, 8, 9, 9, 9, 9, 9, 9, 10, 10,
			10, 10, 11, 11, 12, 12, 12, 12, 8, 9, 9, 9, 9, 9, 9, 10,
			10, 10, 10, 11, 11, 12, 12, 12, 12, 9, 9, 9, 9, 9, 10, 10,
			10, 10, 10, 11, 11, 11, 12, 12, 13, 13, 9, 9, 9, 9, 9, 10,
			10, 10, 10, 11, 10, 11, 11, 12, 12, 13, 13, 10, 10, 10, 10, 10,
			11, 11, 11, 11, 11, 11, 11, 12, 12, 12, 13, 13, 10, 10, 10, 10,
			10, 11, 11, 11, 11, 11, 11, 12, 11, 12, 12, 13, 13, 11, 11, 11,
			11, 11, 11, 11, 12, 12, 12, 12, 12, 12, 13, 13, 13, 13, 11, 11,
			11, 11, 11, 11, 11, 12, 12, 12, 12, 13, 12, 13, 13, 13, 13, 11,
			12, 12, 12, 12, 12, 12, 12, 12, 13, 13, 13, 13, 13, 13, 14, 14,
			11, 12, 12, 12, 12, 12, 12, 12, 13, 13, 13, 13, 13, 13, 13, 14,
			14
		};

		public CodeBookMapType MapType        { get; } = (CodeBookMapType)1;
		public int             QuantMin       { get; } = -529530880;
		public int             QuantDelta     { get; } = 1611661312;
		public int             Quant          { get; } = 5;
		public int             QuantSequenceP { get; } = 0;

		public int[] QuantList { get; } =
		{
			8,
			7,
			9,
			6,
			10,
			5,
			11,
			4,
			12,
			3,
			13,
			2,
			14,
			1,
			15,
			0,
			16
		};
	}
}