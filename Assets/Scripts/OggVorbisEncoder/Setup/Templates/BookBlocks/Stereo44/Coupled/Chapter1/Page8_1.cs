namespace OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Coupled.Chapter1
{
	public class Page81 : IStaticCodeBook
	{
		public int Dimensions { get; } = 2;

		public byte[] LengthList { get; } =
		{
			1, 4, 4, 6, 5, 7, 7, 9, 9, 10, 10, 12, 12, 6, 5, 5,
			7, 7, 8, 8, 10, 10, 12, 11, 12, 12, 6, 5, 5, 7, 7, 8,
			8, 10, 10, 11, 11, 12, 12, 15, 7, 7, 8, 8, 9, 9, 11, 11,
			12, 12, 13, 12, 15, 8, 8, 8, 7, 9, 9, 10, 10, 12, 12, 13,
			13, 16, 11, 10, 8, 8, 10, 10, 11, 11, 12, 12, 13, 13, 16, 11,
			11, 9, 8, 11, 10, 11, 11, 12, 12, 13, 12, 16, 16, 16, 10, 11,
			10, 11, 12, 12, 12, 12, 13, 13, 16, 16, 16, 11, 9, 11, 9, 14,
			12, 12, 12, 13, 13, 16, 16, 16, 12, 14, 11, 12, 12, 12, 13, 13,
			14, 13, 16, 16, 16, 15, 13, 12, 10, 13, 10, 13, 14, 13, 13, 16,
			16, 16, 16, 16, 13, 14, 12, 13, 13, 12, 13, 13, 16, 16, 16, 16,
			16, 13, 12, 12, 11, 14, 12, 15, 13
		};

		public CodeBookMapType MapType        { get; } = (CodeBookMapType)1;
		public int             QuantMin       { get; } = -522616832;
		public int             QuantDelta     { get; } = 1620115456;
		public int             Quant          { get; } = 4;
		public int             QuantSequenceP { get; } = 0;

		public int[] QuantList { get; } =
		{
			6,
			5,
			7,
			4,
			8,
			3,
			9,
			2,
			10,
			1,
			11,
			0,
			12
		};
	}
}