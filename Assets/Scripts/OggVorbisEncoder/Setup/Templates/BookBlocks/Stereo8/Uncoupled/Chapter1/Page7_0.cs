namespace OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo8.Uncoupled.Chapter1
{
	public class Page70 : IStaticCodeBook
	{
		public int Dimensions { get; } = 4;

		public byte[] LengthList { get; } =
		{
			1, 4, 4, 5, 7, 7, 5, 7, 7, 5, 9, 9, 8, 10, 10, 8,
			10, 10, 5, 9, 9, 7, 10, 10, 8, 10, 10, 4, 10, 10, 9, 12,
			12, 9, 11, 11, 7, 12, 11, 10, 11, 13, 10, 13, 13, 7, 12, 12,
			10, 13, 12, 10, 13, 13, 4, 10, 10, 9, 12, 12, 9, 12, 12, 7,
			12, 12, 10, 13, 13, 10, 12, 13, 7, 11, 12, 10, 13, 13, 10, 13,
			11
		};

		public CodeBookMapType MapType        { get; } = (CodeBookMapType)1;
		public int             QuantMin       { get; } = -529137664;
		public int             QuantDelta     { get; } = 1618345984;
		public int             Quant          { get; } = 2;
		public int             QuantSequenceP { get; } = 0;

		public int[] QuantList { get; } =
		{
			1,
			0,
			2
		};
	}
}