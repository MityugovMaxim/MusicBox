namespace OggVorbisEncoder.Setup.Templates.BookBlocks.Stereo44.Uncoupled.Chapter8
{
	public class Page91 : IStaticCodeBook
	{
		public int Dimensions { get; } = 2;

		public byte[] LengthList { get; } =
		{
			1, 4, 4, 7, 7, 8, 7, 8, 6, 9, 7, 10, 8, 11, 10, 11,
			11, 11, 11, 4, 7, 6, 9, 9, 10, 9, 9, 9, 10, 10, 11, 10,
			11, 10, 11, 11, 13, 11, 4, 7, 7, 9, 9, 9, 9, 9, 9, 10,
			10, 11, 10, 11, 11, 11, 12, 11, 12, 7, 9, 8, 11, 11, 11, 11,
			10, 10, 11, 11, 12, 12, 12, 12, 12, 12, 14, 13, 7, 8, 9, 10,
			11, 11, 11, 10, 10, 11, 11, 11, 11, 12, 12, 14, 12, 13, 14, 8,
			9, 9, 11, 11, 11, 11, 11, 11, 12, 12, 14, 12, 15, 14, 14, 14,
			15, 14, 8, 9, 9, 11, 11, 11, 11, 12, 11, 12, 12, 13, 13, 13,
			13, 13, 13, 14, 14, 8, 9, 9, 11, 10, 12, 11, 12, 12, 13, 13,
			13, 13, 15, 14, 14, 14, 16, 16, 8, 9, 9, 10, 11, 11, 12, 12,
			12, 13, 13, 13, 14, 14, 14, 15, 16, 15, 15, 9, 10, 10, 11, 12,
			12, 13, 13, 13, 14, 14, 16, 14, 14, 16, 16, 16, 16, 15, 9, 10,
			10, 11, 11, 12, 13, 13, 14, 15, 14, 16, 14, 15, 16, 16, 16, 16,
			15, 10, 11, 11, 12, 13, 13, 14, 15, 15, 15, 15, 15, 16, 15, 16,
			15, 16, 15, 15, 10, 11, 11, 13, 13, 14, 13, 13, 15, 14, 15, 15,
			16, 15, 15, 15, 16, 15, 16, 10, 12, 12, 14, 14, 14, 14, 14, 16,
			16, 15, 15, 15, 16, 16, 16, 16, 16, 16, 11, 12, 12, 14, 14, 14,
			14, 15, 15, 16, 15, 16, 15, 16, 15, 16, 16, 16, 16, 12, 12, 13,
			14, 14, 15, 16, 16, 16, 16, 16, 16, 15, 16, 16, 16, 16, 16, 16,
			12, 13, 13, 14, 14, 14, 14, 15, 16, 15, 16, 16, 16, 16, 16, 16,
			16, 16, 16, 12, 13, 14, 14, 14, 16, 15, 16, 15, 16, 16, 16, 16,
			16, 16, 16, 16, 16, 16, 12, 14, 13, 14, 15, 15, 15, 16, 15, 16,
			16, 15, 16, 16, 16, 16, 16, 16, 16
		};

		public CodeBookMapType MapType        { get; } = (CodeBookMapType)1;
		public int             QuantMin       { get; } = -518287360;
		public int             QuantDelta     { get; } = 1622704128;
		public int             Quant          { get; } = 5;
		public int             QuantSequenceP { get; } = 0;

		public int[] QuantList { get; } =
		{
			9,
			8,
			10,
			7,
			11,
			6,
			12,
			5,
			13,
			4,
			14,
			3,
			15,
			2,
			16,
			1,
			17,
			0,
			18
		};
	}
}