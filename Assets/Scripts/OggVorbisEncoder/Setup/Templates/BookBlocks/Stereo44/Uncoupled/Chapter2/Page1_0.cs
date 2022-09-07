namespace OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Uncoupled.Chapter2
{
	public class Page10 : IStaticCodeBook
	{
		public int Dimensions { get; } = 4;

		public byte[] LengthList { get; } =
		{
			1, 4, 4, 5, 8, 7, 5, 7, 8, 5, 8, 8, 8, 11, 11, 8,
			10, 11, 5, 8, 8, 8, 11, 10, 8, 11, 11, 4, 8, 8, 8, 11,
			11, 8, 11, 11, 8, 11, 11, 11, 13, 14, 11, 13, 13, 7, 11, 11,
			10, 13, 12, 11, 14, 14, 4, 8, 8, 8, 11, 11, 8, 11, 11, 8,
			11, 11, 11, 14, 13, 10, 12, 13, 8, 11, 11, 11, 13, 13, 11, 13,
			13
		};

		public CodeBookMapType MapType        { get; } = (CodeBookMapType)1;
		public int             QuantMin       { get; } = -535822336;
		public int             QuantDelta     { get; } = 1611661312;
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