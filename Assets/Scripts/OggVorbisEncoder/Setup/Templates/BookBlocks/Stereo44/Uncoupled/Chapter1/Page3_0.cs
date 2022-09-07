namespace OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Uncoupled.Chapter1
{
	public class Page30 : IStaticCodeBook
	{
		public int Dimensions { get; } = 4;

		public byte[] LengthList { get; } =
		{
			1, 5, 5, 8, 8, 5, 8, 7, 9, 9, 5, 7, 8, 9, 9, 9,
			10, 9, 12, 12, 9, 9, 10, 12, 12, 6, 8, 8, 11, 10, 8, 10,
			10, 11, 11, 8, 9, 10, 11, 11, 10, 11, 11, 14, 13, 10, 11, 11,
			13, 13, 5, 8, 8, 10, 10, 8, 10, 10, 11, 11, 8, 10, 10, 11,
			11, 10, 11, 11, 13, 13, 10, 11, 11, 13, 13, 9, 11, 11, 15, 14,
			10, 12, 12, 15, 14, 10, 12, 11, 15, 14, 13, 14, 14, 16, 16, 12,
			14, 13, 17, 15, 9, 11, 11, 14, 15, 10, 11, 12, 14, 16, 10, 11,
			12, 14, 16, 12, 13, 14, 16, 16, 13, 13, 15, 15, 18, 5, 8, 8,
			11, 11, 8, 10, 10, 12, 12, 8, 10, 10, 12, 13, 11, 12, 12, 14,
			14, 11, 12, 12, 15, 15, 8, 10, 10, 13, 13, 10, 12, 12, 13, 13,
			10, 12, 12, 14, 14, 12, 13, 13, 15, 15, 12, 13, 13, 16, 16, 7,
			10, 10, 12, 12, 10, 12, 11, 13, 13, 10, 12, 12, 13, 14, 12, 13,
			12, 15, 14, 12, 13, 13, 16, 16, 10, 12, 12, 17, 16, 12, 13, 13,
			16, 15, 11, 13, 13, 17, 17, 15, 15, 15, 16, 17, 14, 15, 15, 19,
			19, 10, 12, 12, 15, 16, 11, 13, 12, 15, 18, 11, 13, 13, 16, 16,
			14, 15, 15, 17, 17, 14, 15, 15, 17, 19, 5, 8, 8, 11, 11, 8,
			10, 10, 12, 12, 8, 10, 10, 12, 12, 11, 12, 12, 16, 15, 11, 12,
			12, 14, 15, 7, 10, 10, 13, 13, 10, 12, 12, 14, 13, 10, 11, 12,
			13, 13, 12, 13, 13, 16, 16, 12, 12, 13, 15, 15, 8, 10, 10, 13,
			13, 10, 12, 12, 14, 14, 10, 12, 12, 13, 13, 12, 13, 13, 16, 16,
			12, 13, 13, 15, 15, 10, 12, 12, 16, 15, 11, 13, 13, 17, 16, 11,
			12, 13, 16, 15, 13, 15, 15, 19, 17, 14, 15, 14, 17, 16, 10, 12,
			12, 16, 16, 11, 13, 13, 16, 17, 12, 13, 13, 15, 17, 14, 15, 15,
			17, 19, 14, 15, 15, 17, 17, 8, 11, 11, 16, 16, 10, 13, 12, 17,
			17, 10, 12, 13, 16, 16, 15, 17, 16, 20, 19, 14, 15, 17, 18, 19,
			9, 12, 12, 16, 17, 11, 13, 14, 17, 18, 11, 13, 13, 19, 18, 16,
			17, 18, 19, 19, 15, 16, 16, 19, 19, 9, 12, 12, 16, 17, 11, 14,
			13, 18, 17, 11, 13, 13, 17, 17, 16, 17, 16, 20, 19, 14, 16, 16,
			18, 18, 12, 15, 15, 19, 17, 14, 15, 16, 0, 20, 13, 15, 16, 20,
			17, 18, 16, 20, 0, 0, 15, 16, 19, 20, 0, 12, 15, 14, 18, 19,
			13, 16, 15, 20, 19, 13, 16, 15, 20, 18, 17, 18, 17, 0, 20, 16,
			17, 16, 0, 0, 8, 11, 11, 16, 15, 10, 12, 12, 17, 17, 10, 13,
			13, 17, 16, 14, 16, 15, 18, 20, 15, 16, 16, 19, 19, 9, 12, 12,
			16, 16, 11, 13, 13, 17, 16, 11, 13, 14, 17, 18, 15, 15, 16, 20,
			20, 16, 16, 17, 19, 19, 9, 13, 12, 16, 17, 11, 14, 13, 17, 17,
			11, 14, 14, 18, 17, 14, 16, 15, 18, 19, 16, 17, 18, 18, 19, 12,
			14, 15, 19, 18, 13, 15, 16, 18, 0, 13, 14, 15, 0, 0, 16, 16,
			17, 20, 0, 17, 17, 20, 20, 0, 12, 15, 15, 19, 20, 13, 15, 15,
			0, 0, 14, 16, 15, 0, 0, 15, 18, 16, 0, 0, 17, 18, 16, 0,
			19
		};

		public CodeBookMapType MapType        { get; } = (CodeBookMapType)1;
		public int             QuantMin       { get; } = -533725184;
		public int             QuantDelta     { get; } = 1611661312;
		public int             Quant          { get; } = 3;
		public int             QuantSequenceP { get; } = 0;

		public int[] QuantList { get; } =
		{
			2,
			1,
			3,
			0,
			4
		};
	}
}