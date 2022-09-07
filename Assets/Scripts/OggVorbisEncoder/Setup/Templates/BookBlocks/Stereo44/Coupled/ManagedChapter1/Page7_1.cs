namespace OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Coupled.ManagedChapter1
{
	public class Page71 : IStaticCodeBook
	{
		public int Dimensions { get; } = 2;

		public byte[] LengthList { get; } =
		{
			2, 4, 4, 4, 5, 6, 5, 5, 5, 5, 6, 5, 5, 5, 5, 6,
			5, 5, 5, 5, 6, 6, 6, 5, 5
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