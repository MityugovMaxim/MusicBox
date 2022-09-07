namespace OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Coupled.Chapter3
{
	public class Page92 : IStaticCodeBook
	{
		public int Dimensions { get; } = 2;

		public byte[] LengthList { get; } =
		{
			2, 5, 5, 6, 6, 7, 7, 7, 7, 7, 7, 8, 8, 8, 8, 8,
			8, 10, 6, 6, 7, 7, 8, 7, 8, 8, 8, 8, 8, 9, 9, 9,
			9, 9, 10, 6, 6, 7, 7, 7, 7, 8, 8, 8, 8, 9, 9, 9,
			9, 9, 9, 10, 7, 7, 7, 7, 8, 8, 8, 8, 9, 9, 9, 9,
			9, 9, 9, 9, 10, 10, 10, 7, 7, 8, 8, 8, 9, 9, 9, 9,
			9, 9, 9, 9, 9, 11, 11, 11, 8, 8, 8, 8, 9, 9, 9, 9,
			9, 9, 9, 9, 9, 9, 10, 10, 10, 8, 8, 8, 8, 9, 9, 9,
			9, 9, 9, 9, 9, 9, 9, 10, 10, 10, 8, 9, 9, 9, 9, 9,
			9, 9, 9, 9, 9, 9, 10, 9, 10, 10, 10, 11, 11, 9, 9, 9,
			9, 9, 9, 9, 9, 9, 9, 9, 9, 11, 10, 11, 11, 11, 9, 9,
			9, 9, 9, 9, 10, 10, 9, 9, 10, 9, 11, 10, 11, 11, 11, 9,
			9, 9, 9, 9, 9, 9, 9, 10, 10, 10, 9, 11, 11, 11, 11, 11,
			9, 9, 9, 9, 10, 10, 9, 9, 9, 9, 10, 9, 11, 11, 11, 11,
			11, 11, 11, 9, 9, 9, 9, 9, 9, 10, 10, 10, 10, 11, 11, 11,
			11, 11, 11, 11, 10, 9, 10, 10, 9, 10, 9, 9, 10, 9, 11, 10,
			10, 11, 11, 11, 11, 9, 10, 9, 9, 9, 9, 10, 10, 10, 10, 11,
			11, 11, 11, 11, 11, 10, 10, 10, 9, 9, 10, 9, 10, 9, 10, 10,
			10, 10, 11, 11, 11, 11, 11, 11, 11, 9, 9, 9, 9, 9, 10, 10,
			10
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