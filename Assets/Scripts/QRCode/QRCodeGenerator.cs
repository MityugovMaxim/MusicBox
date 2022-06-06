using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;

public class QRCodeException : Exception
{
	public QRCodeException(string _ErrorCorrection, string _EncodingMode, int _MaxSize) : base(
		$"The given payload exceeds the maximum size of the QR code standard. The maximum size allowed for the choosen paramters (ECC level={_ErrorCorrection}, EncodingMode={_EncodingMode}) is {_MaxSize} byte."
	) { }

	public QRCodeException(string _ErrorCorrection, string _EncodingMode, int _Version, int _MaxSize) : base(
		$"The given payload exceeds the maximum size of the QR code standard. The maximum size allowed for the choosen paramters (ECC level={_ErrorCorrection}, EncodingMode={_EncodingMode}, FixedVersion={_Version}) is {_MaxSize} byte."
	) { }
}

public class QRCodeData
{
	public List<BitArray> Matrix { get; }

	public QRCodeData(int _Version)
	{
		int size = ModulesPerSideFromVersion(_Version);
		Matrix = new List<BitArray>();
		for (var i = 0; i < size; i++)
			Matrix.Add(new BitArray(size));
	}

	static int ModulesPerSideFromVersion(int _Version)
	{
		return 21 + (_Version - 1) * 4;
	}
}

public static class QRCodeGenerator
{
	public enum ErrorCorrection
	{
		L,
		M,
		Q,
		H
	}

	enum EncodingMode
	{
		Numeric      = 1,
		Alphanumeric = 2,
		Byte         = 4,
		Kanji        = 8,
		ECI          = 7
	}

	static readonly char[] m_AlphanumEncTable = { ' ', '$', '%', '*', '+', '-', '.', '/', ':' };

	static readonly int[] m_CapacityBaseValues =
	{
		41, 25, 17, 10, 34, 20, 14, 8, 27, 16, 11, 7, 17, 10, 7, 4, 77, 47, 32, 20, 63, 38, 26, 16, 48, 29, 20, 12, 34, 20, 14, 8, 127, 77, 53, 32, 101, 61, 42, 26, 77, 47, 32, 20, 58,
		35, 24, 15, 187, 114, 78, 48, 149, 90, 62, 38, 111, 67, 46, 28, 82, 50, 34, 21, 255, 154, 106, 65, 202, 122, 84, 52, 144, 87, 60, 37, 106, 64, 44, 27, 322, 195, 134, 82, 255,
		154, 106, 65, 178, 108, 74, 45, 139, 84, 58, 36, 370, 224, 154, 95, 293, 178, 122, 75, 207, 125, 86, 53, 154, 93, 64, 39, 461, 279, 192, 118, 365, 221, 152, 93, 259, 157, 108,
		66, 202, 122, 84, 52, 552, 335, 230, 141, 432, 262, 180, 111, 312, 189, 130, 80, 235, 143, 98, 60, 652, 395, 271, 167, 513, 311, 213, 131, 364, 221, 151, 93, 288, 174, 119, 74,
		772, 468, 321, 198, 604, 366, 251, 155, 427, 259, 177, 109, 331, 200, 137, 85, 883, 535, 367, 226, 691, 419, 287, 177, 489, 296, 203, 125, 374, 227, 155, 96, 1022, 619, 425, 262,
		796, 483, 331, 204, 580, 352, 241, 149, 427, 259, 177, 109, 1101, 667, 458, 282, 871, 528, 362, 223, 621, 376, 258, 159, 468, 283, 194, 120, 1250, 758, 520, 320, 991, 600, 412,
		254, 703, 426, 292, 180, 530, 321, 220, 136, 1408, 854, 586, 361, 1082, 656, 450, 277, 775, 470, 322, 198, 602, 365, 250, 154, 1548, 938, 644, 397, 1212, 734, 504, 310, 876, 531,
		364, 224, 674, 408, 280, 173, 1725, 1046, 718, 442, 1346, 816, 560, 345, 948, 574, 394, 243, 746, 452, 310, 191, 1903, 1153, 792, 488, 1500, 909, 624, 384, 1063, 644, 442, 272,
		813, 493, 338, 208, 2061, 1249, 858, 528, 1600, 970, 666, 410, 1159, 702, 482, 297, 919, 557, 382, 235, 2232, 1352, 929, 572, 1708, 1035, 711, 438, 1224, 742, 509, 314, 969, 587,
		403, 248, 2409, 1460, 1003, 618, 1872, 1134, 779, 480, 1358, 823, 565, 348, 1056, 640, 439, 270, 2620, 1588, 1091, 672, 2059, 1248, 857, 528, 1468, 890, 611, 376, 1108, 672, 461,
		284, 2812, 1704, 1171, 721, 2188, 1326, 911, 561, 1588, 963, 661, 407, 1228, 744, 511, 315, 3057, 1853, 1273, 784, 2395, 1451, 997, 614, 1718, 1041, 715, 440, 1286, 779, 535,
		330, 3283, 1990, 1367, 842, 2544, 1542, 1059, 652, 1804, 1094, 751, 462, 1425, 864, 593, 365, 3517, 2132, 1465, 902, 2701, 1637, 1125, 692, 1933, 1172, 805, 496, 1501, 910, 625,
		385, 3669, 2223, 1528, 940, 2857, 1732, 1190, 732, 2085, 1263, 868, 534, 1581, 958, 658, 405, 3909, 2369, 1628, 1002, 3035, 1839, 1264, 778, 2181, 1322, 908, 559, 1677, 1016,
		698, 430, 4158, 2520, 1732, 1066, 3289, 1994, 1370, 843, 2358, 1429, 982, 604, 1782, 1080, 742, 457, 4417, 2677, 1840, 1132, 3486, 2113, 1452, 894, 2473, 1499, 1030, 634, 1897,
		1150, 790, 486, 4686, 2840, 1952, 1201, 3693, 2238, 1538, 947, 2670, 1618, 1112, 684, 2022, 1226, 842, 518, 4965, 3009, 2068, 1273, 3909, 2369, 1628, 1002, 2805, 1700, 1168, 719,
		2157, 1307, 898, 553, 5253, 3183, 2188, 1347, 4134, 2506, 1722, 1060, 2949, 1787, 1228, 756, 2301, 1394, 958, 590, 5529, 3351, 2303, 1417, 4343, 2632, 1809, 1113, 3081, 1867,
		1283, 790, 2361, 1431, 983, 605, 5836, 3537, 2431, 1496, 4588, 2780, 1911, 1176, 3244, 1966, 1351, 832, 2524, 1530, 1051, 647, 6153, 3729, 2563, 1577, 4775, 2894, 1989, 1224,
		3417, 2071, 1423, 876, 2625, 1591, 1093, 673, 6479, 3927, 2699, 1661, 5039, 3054, 2099, 1292, 3599, 2181, 1499, 923, 2735, 1658, 1139, 701, 6743, 4087, 2809, 1729, 5313, 3220,
		2213, 1362, 3791, 2298, 1579, 972, 2927, 1774, 1219, 750, 7089, 4296, 2953, 1817, 5596, 3391, 2331, 1435, 3993, 2420, 1663, 1024, 3057, 1852, 1273, 784
	};

	static readonly int[] m_CapacityEccBaseValues =
	{
		19, 7, 1, 19, 0, 0, 16, 10, 1, 16, 0, 0, 13, 13, 1, 13, 0, 0, 9, 17, 1, 9, 0, 0, 34, 10, 1, 34, 0, 0, 28, 16, 1, 28, 0, 0, 22, 22, 1, 22, 0, 0, 16, 28, 1, 16, 0, 0, 55, 15, 1,
		55, 0, 0, 44, 26, 1, 44, 0, 0, 34, 18, 2, 17, 0, 0, 26, 22, 2, 13, 0, 0, 80, 20, 1, 80, 0, 0, 64, 18, 2, 32, 0, 0, 48, 26, 2, 24, 0, 0, 36, 16, 4, 9, 0, 0, 108, 26, 1, 108, 0, 0,
		86, 24, 2, 43, 0, 0, 62, 18, 2, 15, 2, 16, 46, 22, 2, 11, 2, 12, 136, 18, 2, 68, 0, 0, 108, 16, 4, 27, 0, 0, 76, 24, 4, 19, 0, 0, 60, 28, 4, 15, 0, 0, 156, 20, 2, 78, 0, 0, 124,
		18, 4, 31, 0, 0, 88, 18, 2, 14, 4, 15, 66, 26, 4, 13, 1, 14, 194, 24, 2, 97, 0, 0, 154, 22, 2, 38, 2, 39, 110, 22, 4, 18, 2, 19, 86, 26, 4, 14, 2, 15, 232, 30, 2, 116, 0, 0, 182,
		22, 3, 36, 2, 37, 132, 20, 4, 16, 4, 17, 100, 24, 4, 12, 4, 13, 274, 18, 2, 68, 2, 69, 216, 26, 4, 43, 1, 44, 154, 24, 6, 19, 2, 20, 122, 28, 6, 15, 2, 16, 324, 20, 4, 81, 0, 0,
		254, 30, 1, 50, 4, 51, 180, 28, 4, 22, 4, 23, 140, 24, 3, 12, 8, 13, 370, 24, 2, 92, 2, 93, 290, 22, 6, 36, 2, 37, 206, 26, 4, 20, 6, 21, 158, 28, 7, 14, 4, 15, 428, 26, 4, 107,
		0, 0, 334, 22, 8, 37, 1, 38, 244, 24, 8, 20, 4, 21, 180, 22, 12, 11, 4, 12, 461, 30, 3, 115, 1, 116, 365, 24, 4, 40, 5, 41, 261, 20, 11, 16, 5, 17, 197, 24, 11, 12, 5, 13, 523,
		22, 5, 87, 1, 88, 415, 24, 5, 41, 5, 42, 295, 30, 5, 24, 7, 25, 223, 24, 11, 12, 7, 13, 589, 24, 5, 98, 1, 99, 453, 28, 7, 45, 3, 46, 325, 24, 15, 19, 2, 20, 253, 30, 3, 15, 13,
		16, 647, 28, 1, 107, 5, 108, 507, 28, 10, 46, 1, 47, 367, 28, 1, 22, 15, 23, 283, 28, 2, 14, 17, 15, 721, 30, 5, 120, 1, 121, 563, 26, 9, 43, 4, 44, 397, 28, 17, 22, 1, 23, 313,
		28, 2, 14, 19, 15, 795, 28, 3, 113, 4, 114, 627, 26, 3, 44, 11, 45, 445, 26, 17, 21, 4, 22, 341, 26, 9, 13, 16, 14, 861, 28, 3, 107, 5, 108, 669, 26, 3, 41, 13, 42, 485, 30, 15,
		24, 5, 25, 385, 28, 15, 15, 10, 16, 932, 28, 4, 116, 4, 117, 714, 26, 17, 42, 0, 0, 512, 28, 17, 22, 6, 23, 406, 30, 19, 16, 6, 17, 1006, 28, 2, 111, 7, 112, 782, 28, 17, 46, 0,
		0, 568, 30, 7, 24, 16, 25, 442, 24, 34, 13, 0, 0, 1094, 30, 4, 121, 5, 122, 860, 28, 4, 47, 14, 48, 614, 30, 11, 24, 14, 25, 464, 30, 16, 15, 14, 16, 1174, 30, 6, 117, 4, 118,
		914, 28, 6, 45, 14, 46, 664, 30, 11, 24, 16, 25, 514, 30, 30, 16, 2, 17, 1276, 26, 8, 106, 4, 107, 1000, 28, 8, 47, 13, 48, 718, 30, 7, 24, 22, 25, 538, 30, 22, 15, 13, 16, 1370,
		28, 10, 114, 2, 115, 1062, 28, 19, 46, 4, 47, 754, 28, 28, 22, 6, 23, 596, 30, 33, 16, 4, 17, 1468, 30, 8, 122, 4, 123, 1128, 28, 22, 45, 3, 46, 808, 30, 8, 23, 26, 24, 628, 30,
		12, 15, 28, 16, 1531, 30, 3, 117, 10, 118, 1193, 28, 3, 45, 23, 46, 871, 30, 4, 24, 31, 25, 661, 30, 11, 15, 31, 16, 1631, 30, 7, 116, 7, 117, 1267, 28, 21, 45, 7, 46, 911, 30,
		1, 23, 37, 24, 701, 30, 19, 15, 26, 16, 1735, 30, 5, 115, 10, 116, 1373, 28, 19, 47, 10, 48, 985, 30, 15, 24, 25, 25, 745, 30, 23, 15, 25, 16, 1843, 30, 13, 115, 3, 116, 1455,
		28, 2, 46, 29, 47, 1033, 30, 42, 24, 1, 25, 793, 30, 23, 15, 28, 16, 1955, 30, 17, 115, 0, 0, 1541, 28, 10, 46, 23, 47, 1115, 30, 10, 24, 35, 25, 845, 30, 19, 15, 35, 16, 2071,
		30, 17, 115, 1, 116, 1631, 28, 14, 46, 21, 47, 1171, 30, 29, 24, 19, 25, 901, 30, 11, 15, 46, 16, 2191, 30, 13, 115, 6, 116, 1725, 28, 14, 46, 23, 47, 1231, 30, 44, 24, 7, 25,
		961, 30, 59, 16, 1, 17, 2306, 30, 12, 121, 7, 122, 1812, 28, 12, 47, 26, 48, 1286, 30, 39, 24, 14, 25, 986, 30, 22, 15, 41, 16, 2434, 30, 6, 121, 14, 122, 1914, 28, 6, 47, 34,
		48, 1354, 30, 46, 24, 10, 25, 1054, 30, 2, 15, 64, 16, 2566, 30, 17, 122, 4, 123, 1992, 28, 29, 46, 14, 47, 1426, 30, 49, 24, 10, 25, 1096, 30, 24, 15, 46, 16, 2702, 30, 4, 122,
		18, 123, 2102, 28, 13, 46, 32, 47, 1502, 30, 48, 24, 14, 25, 1142, 30, 42, 15, 32, 16, 2812, 30, 20, 117, 4, 118, 2216, 28, 40, 47, 7, 48, 1582, 30, 43, 24, 22, 25, 1222, 30, 10,
		15, 67, 16, 2956, 30, 19, 118, 6, 119, 2334, 28, 18, 47, 31, 48, 1666, 30, 34, 24, 34, 25, 1276, 30, 20, 15, 61, 16
	};

	static readonly int[] m_AlignmentPatternBaseValues =
	{
		0, 0, 0, 0, 0, 0, 0, 6, 18, 0, 0, 0, 0, 0, 6, 22, 0, 0, 0, 0, 0, 6, 26, 0, 0, 0, 0, 0, 6, 30, 0, 0, 0, 0, 0, 6, 34, 0, 0, 0, 0, 0, 6, 22, 38, 0, 0, 0, 0, 6, 24, 42, 0, 0, 0, 0,
		6, 26, 46, 0, 0, 0, 0, 6, 28, 50, 0, 0, 0, 0, 6, 30, 54, 0, 0, 0, 0, 6, 32, 58, 0, 0, 0, 0, 6, 34, 62, 0, 0, 0, 0, 6, 26, 46, 66, 0, 0, 0, 6, 26, 48, 70, 0, 0, 0, 6, 26, 50, 74,
		0, 0, 0, 6, 30, 54, 78, 0, 0, 0, 6, 30, 56, 82, 0, 0, 0, 6, 30, 58, 86, 0, 0, 0, 6, 34, 62, 90, 0, 0, 0, 6, 28, 50, 72, 94, 0, 0, 6, 26, 50, 74, 98, 0, 0, 6, 30, 54, 78, 102, 0,
		0, 6, 28, 54, 80, 106, 0, 0, 6, 32, 58, 84, 110, 0, 0, 6, 30, 58, 86, 114, 0, 0, 6, 34, 62, 90, 118, 0, 0, 6, 26, 50, 74, 98, 122, 0, 6, 30, 54, 78, 102, 126, 0, 6, 26, 52, 78,
		104, 130, 0, 6, 30, 56, 82, 108, 134, 0, 6, 34, 60, 86, 112, 138, 0, 6, 30, 58, 86, 114, 142, 0, 6, 34, 62, 90, 118, 146, 0, 6, 30, 54, 78, 102, 126, 150, 6, 24, 50, 76, 102,
		128, 154, 6, 28, 54, 80, 106, 132, 158, 6, 32, 58, 84, 110, 136, 162, 6, 26, 54, 82, 110, 138, 166, 6, 30, 58, 86, 114, 142, 170
	};

	static readonly int[] m_RemainderBits = { 0, 7, 7, 7, 7, 7, 0, 0, 0, 0, 0, 0, 0, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 3, 3, 3, 3, 3, 3, 3, 0, 0, 0, 0, 0, 0 };

	static readonly List<AlignmentPattern> m_AlignmentPatternTable = CreateAlignmentPatternTable();
	static readonly List<ErrorCorrectionInfo>          m_CorrectionTable      = CreateCorrectionTable();
	static readonly List<VersionInfo>      m_CapacityTable         = CreateCapacityTable();
	static readonly List<Antilog>          m_GaloisField           = CreateAntilogTable();
	static readonly Dictionary<char, int>  m_AlphanumEncDict       = CreateAlphanumEncDict();

	public static List<BitArray> Generate(string _PlainText, ErrorCorrection _ErrorCorrection)
	{
		QRCodeData data = Generate(_PlainText, _ErrorCorrection, false);
		
		return data.Matrix;
	}

	public static QRCodeData Generate(string _PlainText, ErrorCorrection _ErrorCorrection, bool _ForceUTF8, int _Version = -1)
	{
		EncodingMode encoding        = GetEncodingFromPlaintext(_PlainText, _ForceUTF8);
		string       codedText       = PlainTextToBinary(_PlainText, encoding, _ForceUTF8);
		int          dataInputLength = GetDataLength(encoding, _PlainText, codedText, _ForceUTF8);
		int          version         = _Version;
		
		if (version == -1)
		{
			version = GetVersion(dataInputLength, encoding, _ErrorCorrection);
		}
		else
		{
			int minVersion = GetVersion(dataInputLength, encoding, _ErrorCorrection);
			if (minVersion > version)
			{
				int maxSizeByte = m_CapacityTable[version - 1].Details
					.First(_Value => _Value.ErrorCorrection == _ErrorCorrection)
					.Capacity[encoding];
				
				throw new QRCodeException(_ErrorCorrection.ToString(), encoding.ToString(), version, maxSizeByte);
			}
		}
		
		string modeIndicator = string.Empty;
		
		modeIndicator += GetBinary((int)encoding, 4);
		
		string countIndicator = GetBinary(dataInputLength, GetCountIndicatorLength(version, encoding));
		
		string bitString = modeIndicator + countIndicator;
		
		bitString += codedText;
		
		return Generate(bitString, _ErrorCorrection, version);
	}

	static QRCodeData Generate(string _BitString, ErrorCorrection _ErrorCorrection, int _Version)
	{
		//Fill up data code word
		ErrorCorrectionInfo errorCorrectionInfo    = m_CorrectionTable.Single(_Item => _Item.Version == _Version && _Item.ErrorCorrection == _ErrorCorrection);
		int dataLength = errorCorrectionInfo.TotalDataCodewords * 8;
		int lengthDiff = dataLength - _BitString.Length;
		if (lengthDiff > 0)
			_BitString += new string('0', Math.Min(lengthDiff, 4));
		if (_BitString.Length % 8 != 0)
			_BitString += new string('0', 8 - (_BitString.Length % 8));
		while (_BitString.Length < dataLength)
			_BitString += "1110110000010001";
		if (_BitString.Length > dataLength)
			_BitString = _BitString.Substring(0, dataLength);
		
		//Calculate error correction words
		List<CodewordBlock> codewords = new List<CodewordBlock>(errorCorrectionInfo.BlocksInGroup1 + errorCorrectionInfo.BlocksInGroup2);
		for (var i = 0; i < errorCorrectionInfo.BlocksInGroup1; i++)
		{
			string       bitString    = _BitString.Substring(i * errorCorrectionInfo.CodewordsInGroup1 * 8, errorCorrectionInfo.CodewordsInGroup1 * 8);
			List<string> bitBlockList = BinaryStringToBitBlockList(bitString);
			List<string> wordList     = CalculateCorrectionWords(bitString, errorCorrectionInfo);
			codewords.Add(new CodewordBlock(bitBlockList, wordList));
		}
		
		_BitString = _BitString.Substring(errorCorrectionInfo.BlocksInGroup1 * errorCorrectionInfo.CodewordsInGroup1 * 8);
		for (int i = 0; i < errorCorrectionInfo.BlocksInGroup2; i++)
		{
			string       bitString    = _BitString.Substring(i * errorCorrectionInfo.CodewordsInGroup2 * 8, errorCorrectionInfo.CodewordsInGroup2 * 8);
			List<string> bitBlockList = BinaryStringToBitBlockList(bitString);
			List<string> eccWordList  = CalculateCorrectionWords(bitString, errorCorrectionInfo);
			codewords.Add(new CodewordBlock(bitBlockList, eccWordList));
		}
		
		//Interleave code words
		StringBuilder interleavedBuilder = new StringBuilder();
		for (int i = 0; i < Math.Max(errorCorrectionInfo.CodewordsInGroup1, errorCorrectionInfo.CodewordsInGroup2); i++)
		{
			foreach (CodewordBlock block in codewords.Where(_Block => _Block.CodeWords.Count > i))
				interleavedBuilder.Append(block.CodeWords[i]);
		}
		for (int i = 0; i < errorCorrectionInfo.CorrectionPerBlock; i++)
		{
			foreach (CodewordBlock block in codewords.Where(_Block => _Block.CorrectionWords.Count > i))
				interleavedBuilder.Append(block.CorrectionWords[i]);
		}
		interleavedBuilder.Append(new string('0', m_RemainderBits[_Version - 1]));
		string interleavedData = interleavedBuilder.ToString();
		
		//Place interleaved data on module matrix
		QRCodeData    qr             = new QRCodeData(_Version);
		List<RectInt> blockedModules = new List<RectInt>();
		ModulePlacer.AddAnchors(ref qr, ref blockedModules);
		ModulePlacer.AddSeparators(qr.Matrix.Count, ref blockedModules);
		ModulePlacer.AddAlignment(ref qr, m_AlignmentPatternTable.Where(_Item => _Item.Version == _Version).Select(_Item => _Item.PatternPositions).First(), ref blockedModules);
		ModulePlacer.AddTimings(ref qr, ref blockedModules);
		ModulePlacer.AddDarkModule(ref qr, _Version, ref blockedModules);
		ModulePlacer.AddVersion(qr.Matrix.Count, _Version, ref blockedModules);
		ModulePlacer.AddData(ref qr, interleavedData, ref blockedModules);
		int    maskVersion = ModulePlacer.AddMask(ref qr, _Version, ref blockedModules, _ErrorCorrection);
		string format      = GetFormatString(_ErrorCorrection, maskVersion);
		
		ModulePlacer.PlaceFormat(ref qr, format);
		if (_Version >= 7)
		{
			var versionString = GetVersionString(_Version);
			ModulePlacer.PlaceVersion(ref qr, versionString);
		}
		
		return qr;
	}

	static string GetFormatString(ErrorCorrection _Level, int _MaskVersion)
	{
		string generator = "10100110111";
		string fStrMask  = "101010000010010";
		
		string fStr;
		switch (_Level)
		{
			case ErrorCorrection.L:
				fStr = "01";
				break;
			case ErrorCorrection.M:
				fStr = "00";
				break;
			case ErrorCorrection.Q:
				fStr = "11";
				break;
			default:
				fStr = "10";
				break;
		}
		
		fStr += GetBinary(_MaskVersion, 3);
		string fStrEcc = fStr.PadRight(15, '0').TrimStart('0');
		while (fStrEcc.Length > 10)
		{
			StringBuilder builder = new StringBuilder();
			generator = generator.PadRight(fStrEcc.Length, '0');
			for (int i = 0; i < fStrEcc.Length; i++)
				builder.Append((Convert.ToInt32(fStrEcc[i]) ^ Convert.ToInt32(generator[i])).ToString());
			fStrEcc = builder.ToString().TrimStart('0');
		}
		fStrEcc =  fStrEcc.PadLeft(10, '0');
		fStr    += fStrEcc;
		
		StringBuilder maskBuilder = new StringBuilder();
		for (int i = 0; i < fStr.Length; i++)
			maskBuilder.Append((Convert.ToInt32(fStr[i]) ^ Convert.ToInt32(fStrMask[i])).ToString());
		return maskBuilder.ToString();
	}

	static string GetVersionString(int _Version)
	{
		string generator          = "1111100100101";
		string versionString      = GetBinary(_Version, 6);
		string versionStringError = versionString.PadRight(18, '0').TrimStart('0');
		while (versionStringError.Length > 12)
		{
			StringBuilder builder = new StringBuilder();
			generator = generator.PadRight(versionStringError.Length, '0');
			for (var i = 0; i < versionStringError.Length; i++)
				builder.Append((Convert.ToInt32(versionStringError[i]) ^ Convert.ToInt32(generator[i])).ToString());
			versionStringError = builder.ToString().TrimStart('0');
		}
		versionStringError =  versionStringError.PadLeft(12, '0');
		versionString      += versionStringError;
		return versionString;
	}

	static class ModulePlacer
	{
		static string ReverseString(string _Value)
		{
			if (_Value.Length <= 0)
				return string.Empty;
			
			string result = string.Empty;
			for (int i = _Value.Length - 1; i >= 0; i--)
				result += _Value[i];
			
			return result;
		}

		public static void PlaceVersion(ref QRCodeData _Data, string _Version)
		{
			int size = _Data.Matrix.Count;
			
			string version = ReverseString(_Version);
			
			for (int x = 0; x < 6; x++)
			for (int y = 0; y < 3; y++)
			{
				_Data.Matrix[y + size - 11][x] = version[x * 3 + y] == '1';
				_Data.Matrix[x][y + size - 11] = version[x * 3 + y] == '1';
			}
		}

		public static void PlaceFormat(ref QRCodeData _Data, string _Format)
		{
			int    size   = _Data.Matrix.Count;
			string format = ReverseString(_Format);
			int[,] modules =
			{
				{ 8, 0, size - 1, 8 },
				{ 8, 1, size - 2, 8 },
				{ 8, 2, size - 3, 8 },
				{ 8, 3, size - 4, 8 },
				{ 8, 4, size - 5, 8 },
				{ 8, 5, size - 6, 8 },
				{ 8, 7, size - 7, 8 },
				{ 8, 8, size - 8, 8 },
				{ 7, 8, 8, size - 7 },
				{ 5, 8, 8, size - 6 },
				{ 4, 8, 8, size - 5 },
				{ 3, 8, 8, size - 4 },
				{ 2, 8, 8, size - 3 },
				{ 1, 8, 8, size - 2 },
				{ 0, 8, 8, size - 1 }
			};
			for (int i = 0; i < 15; i++)
			{
				Vector2Int a = new Vector2Int(modules[i, 0], modules[i, 1]);
				Vector2Int b = new Vector2Int(modules[i, 2], modules[i, 3]);
				_Data.Matrix[a.y][a.x] = format[i] == '1';
				_Data.Matrix[b.y][b.x] = format[i] == '1';
			}
		}

		public static int AddMask(ref QRCodeData _Data, int _Version, ref List<RectInt> _Modules, ErrorCorrection _ErrorCorrection)
		{
			int? selectedPattern = null;
			int  patternScore    = 0;
			
			int size = _Data.Matrix.Count;
			
			var patterns = new Dictionary<int, Func<int, int, bool>>(8)
			{
				{ 1, MaskPattern.Pattern1 },
				{ 2, MaskPattern.Pattern2 },
				{ 3, MaskPattern.Pattern3 },
				{ 4, MaskPattern.Pattern4 },
				{ 5, MaskPattern.Pattern5 },
				{ 6, MaskPattern.Pattern6 },
				{ 7, MaskPattern.Pattern7 },
				{ 8, MaskPattern.Pattern8 }
			};
			
			foreach (var pattern in patterns)
			{
				QRCodeData buffer = new QRCodeData(_Version);
				for (int y = 0; y < size; y++)
				for (int x = 0; x < size; x++)
					buffer.Matrix[y][x] = _Data.Matrix[y][x];
				
				string format = GetFormatString(_ErrorCorrection, pattern.Key - 1);
				ModulePlacer.PlaceFormat(ref buffer, format);
				
				if (_Version >= 7)
				{
					string version = GetVersionString(_Version);
					ModulePlacer.PlaceVersion(ref buffer, version);
				}
				
				for (int x = 0; x < size; x++)
				{
					for (int y = 0; y < x; y++)
					{
						if (IsBlocked(new RectInt(x, y, 1, 1), _Modules))
							continue;
						
						buffer.Matrix[y][x] ^= pattern.Value(x, y);
						buffer.Matrix[x][y] ^= pattern.Value(y, x);
					}
					
					if (IsBlocked(new RectInt(x, x, 1, 1), _Modules))
						continue;
					
					buffer.Matrix[x][x] ^= pattern.Value(x, x);
				}
				
				int score = MaskPattern.Score(ref buffer);
				
				if (selectedPattern.HasValue && patternScore <= score)
					continue;
				
				selectedPattern = pattern.Key;
				patternScore    = score;
			}
			
			if (selectedPattern == null)
				return 0;
			
			for (int x = 0; x < size; x++)
			{
				for (int y = 0; y < x; y++)
				{
					if (IsBlocked(new RectInt(x, y, 1, 1), _Modules))
						continue;
					
					_Data.Matrix[y][x] ^= patterns[selectedPattern.Value](x, y);
					_Data.Matrix[x][y] ^= patterns[selectedPattern.Value](y, x);
				}
				
				if (IsBlocked(new RectInt(x, x, 1, 1), _Modules))
					continue;
				
				_Data.Matrix[x][x] ^= patterns[selectedPattern.Value](x, x);
			}
			
			return selectedPattern.Value - 1;
		}

		public static void AddData(ref QRCodeData _Data, string _BitString, ref List<RectInt> _Modules)
		{
			int         size  = _Data.Matrix.Count;
			bool        up    = true;
			Queue<bool> words = new Queue<bool>();
			foreach (char bit in _BitString)
				words.Enqueue(bit != '0');
			for (int x = size - 1; x >= 0; x -= 2)
			{
				if (x == 6)
					x = 5;
				for (int yMod = 1; yMod <= size; yMod++)
				{
					int y;
					if (up)
					{
						y = size - yMod;
						if (words.Count > 0 && !IsBlocked(new RectInt(x, y, 1, 1), _Modules))
							_Data.Matrix[y][x] = words.Dequeue();
						if (words.Count > 0 && x > 0 && !IsBlocked(new RectInt(x - 1, y, 1, 1), _Modules))
							_Data.Matrix[y][x - 1] = words.Dequeue();
					}
					else
					{
						y = yMod - 1;
						if (words.Count > 0 && !IsBlocked(new RectInt(x, y, 1, 1), _Modules))
							_Data.Matrix[y][x] = words.Dequeue();
						if (words.Count > 0 && x > 0 && !IsBlocked(new RectInt(x - 1, y, 1, 1), _Modules))
							_Data.Matrix[y][x - 1] = words.Dequeue();
					}
				}
				up = !up;
			}
		}

		public static void AddSeparators(int _Size, ref List<RectInt> _Modules)
		{
			_Modules.AddRange(
				new[]
				{
					new RectInt(7, 0, 1, 8),
					new RectInt(0, 7, 7, 1),
					new RectInt(0, _Size - 8, 8, 1),
					new RectInt(7, _Size - 7, 1, 7),
					new RectInt(_Size - 8, 0, 1, 8),
					new RectInt(_Size - 7, 7, 7, 1)
				}
			);
		}

		public static void AddVersion(int _Size, int _Version, ref List<RectInt> _Modules)
		{
			_Modules.AddRange(
				new[]
				{
					new RectInt(8, 0, 1, 6),
					new RectInt(8, 7, 1, 1),
					new RectInt(0, 8, 6, 1),
					new RectInt(7, 8, 2, 1),
					new RectInt(_Size - 8, 8, 8, 1),
					new RectInt(8, _Size - 7, 1, 7)
				}
			);
			
			if (_Version >= 7)
			{
				_Modules.AddRange(
					new[]
					{
						new RectInt(_Size - 11, 0, 3, 6),
						new RectInt(0, _Size - 11, 6, 3)
					}
				);
			}
		}

		public static void AddDarkModule(ref QRCodeData _Data, int _Version, ref List<RectInt> _Modules)
		{
			_Data.Matrix[4 * _Version + 9][8] = true;
			_Modules.Add(new RectInt(8, 4 * _Version + 9, 1, 1));
		}

		public static void AddAnchors(ref QRCodeData _Data, ref List<RectInt> _Modules)
		{
			int   size      = _Data.Matrix.Count;
			int[] locations = { 0, 0, size - 7, 0, 0, size - 7 };
			for (int i = 0; i < 6; i += 2)
			{
				for (int x = 0; x < 7; x++)
				for (int y = 0; y < 7; y++)
				{
					if (((x == 1 || x == 5) && y > 0 && y < 6) || (x > 0 && x < 6 && (y == 1 || y == 5)))
						continue;
					
					_Data.Matrix[y + locations[i + 1]][x + locations[i]] = true;
				}
				_Modules.Add(new RectInt(locations[i], locations[i + 1], 7, 7));
			}
		}

		public static void AddAlignment(ref QRCodeData _Data, List<Vector2Int> _Points, ref List<RectInt> _Modules)
		{
			foreach (Vector2Int point in _Points)
			{
				RectInt rect    = new RectInt(point.x, point.y, 5, 5);
				bool    blocked = false;
				foreach (RectInt blockedRect in _Modules)
				{
					if (!Intersects(rect, blockedRect))
						continue;
					
					blocked = true;
					
					break;
				}
				
				if (blocked)
					continue;
				
				for (var x = 0; x < 5; x++)
				for (var y = 0; y < 5; y++)
				{
					if (y == 0 || y == 4 || x == 0 || x == 4 || (x == 2 && y == 2))
						_Data.Matrix[point.y + y][point.x + x] = true;
				}
				_Modules.Add(new RectInt(point.x, point.y, 5, 5));
			}
		}

		public static void AddTimings(ref QRCodeData _Data, ref List<RectInt> _Modules)
		{
			int size = _Data.Matrix.Count;
			for (int i = 8; i < size - 8; i++)
			{
				if (i % 2 == 0)
				{
					_Data.Matrix[6][i] = true;
					_Data.Matrix[i][6] = true;
				}
			}
			
			_Modules.AddRange(
				new[]
				{
					new RectInt(6, 8, 1, size - 16),
					new RectInt(8, 6, size - 16, 1)
				}
			);
		}

		static bool Intersects(RectInt _A, RectInt _B)
		{
			return _B.x < _A.x + _A.width && _A.x < _B.x + _B.width && _B.y < _A.y + _A.height && _A.y < _B.y + _B.height;
		}

		static bool IsBlocked(RectInt _Rect, List<RectInt> _Modules)
		{
			foreach (var blockedMod in _Modules)
			{
				if (Intersects(blockedMod, _Rect))
					return true;
			}
			return false;
		}

		static class MaskPattern
		{
			public static bool Pattern1(int _X, int _Y) => (_X + _Y) % 2 == 0;

			public static bool Pattern2(int _X, int _Y) => _Y % 2 == 0;

			public static bool Pattern3(int _X, int _Y) => _X % 3 == 0;

			public static bool Pattern4(int _X, int _Y) => (_X + _Y) % 3 == 0;

			public static bool Pattern5(int _X, int _Y) => (int)(Math.Floor(_Y / 2d) + Math.Floor(_X / 3d)) % 2 == 0;

			public static bool Pattern6(int _X, int _Y) => _X * _Y % 2 + _X * _Y % 3 == 0;

			public static bool Pattern7(int _X, int _Y) => (_X * _Y % 2 + _X * _Y % 3) % 2 == 0;

			public static bool Pattern8(int _X, int _Y) => ((_X + _Y) % 2 + _X * _Y % 3) % 2 == 0;

			public static int Score(ref QRCodeData _Data)
			{
				int score1 = 0;
				int score2 = 0;
				int score3 = 0;
				int score4;
				int size = _Data.Matrix.Count;
				
				//Penalty 1
				for (int y = 0; y < size; y++)
				{
					int modInRow      = 0;
					int modInColumn   = 0;
					bool lastValRow    = _Data.Matrix[y][0];
					bool lastValColumn = _Data.Matrix[0][y];
					for (int x = 0; x < size; x++)
					{
						if (_Data.Matrix[y][x] == lastValRow)
							modInRow++;
						else
							modInRow = 1;
						if (modInRow == 5)
							score1 += 3;
						else if (modInRow > 5)
							score1++;
						lastValRow = _Data.Matrix[y][x];
						
						if (_Data.Matrix[x][y] == lastValColumn)
							modInColumn++;
						else
							modInColumn = 1;
						if (modInColumn == 5)
							score1 += 3;
						else if (modInColumn > 5)
							score1++;
						lastValColumn = _Data.Matrix[x][y];
					}
				}
				
				//Penalty 2
				for (int y = 0; y < size - 1; y++)
				for (int x = 0; x < size - 1; x++)
				{
					if (_Data.Matrix[y][x] == _Data.Matrix[y][x + 1] &&
						_Data.Matrix[y][x] == _Data.Matrix[y + 1][x] &&
						_Data.Matrix[y][x] == _Data.Matrix[y + 1][x + 1])
						score2 += 3;
				}
				
				//Penalty 3
				for (int y = 0; y < size; y++)
				for (int x = 0; x < size - 10; x++)
				{
					if ((_Data.Matrix[y][x] &&
							!_Data.Matrix[y][x + 1] &&
							_Data.Matrix[y][x + 2] &&
							_Data.Matrix[y][x + 3] &&
							_Data.Matrix[y][x + 4] &&
							!_Data.Matrix[y][x + 5] &&
							_Data.Matrix[y][x + 6] &&
							!_Data.Matrix[y][x + 7] &&
							!_Data.Matrix[y][x + 8] &&
							!_Data.Matrix[y][x + 9] &&
							!_Data.Matrix[y][x + 10]) ||
						(!_Data.Matrix[y][x] &&
							!_Data.Matrix[y][x + 1] &&
							!_Data.Matrix[y][x + 2] &&
							!_Data.Matrix[y][x + 3] &&
							_Data.Matrix[y][x + 4] &&
							!_Data.Matrix[y][x + 5] &&
							_Data.Matrix[y][x + 6] &&
							_Data.Matrix[y][x + 7] &&
							_Data.Matrix[y][x + 8] &&
							!_Data.Matrix[y][x + 9] &&
							_Data.Matrix[y][x + 10]))
					{
						score3 += 40;
					}
					
					if ((_Data.Matrix[x][y] &&
							!_Data.Matrix[x + 1][y] &&
							_Data.Matrix[x + 2][y] &&
							_Data.Matrix[x + 3][y] &&
							_Data.Matrix[x + 4][y] &&
							!_Data.Matrix[x + 5][y] &&
							_Data.Matrix[x + 6][y] &&
							!_Data.Matrix[x + 7][y] &&
							!_Data.Matrix[x + 8][y] &&
							!_Data.Matrix[x + 9][y] &&
							!_Data.Matrix[x + 10][y]) ||
						(!_Data.Matrix[x][y] &&
							!_Data.Matrix[x + 1][y] &&
							!_Data.Matrix[x + 2][y] &&
							!_Data.Matrix[x + 3][y] &&
							_Data.Matrix[x + 4][y] &&
							!_Data.Matrix[x + 5][y] &&
							_Data.Matrix[x + 6][y] &&
							_Data.Matrix[x + 7][y] &&
							_Data.Matrix[x + 8][y] &&
							!_Data.Matrix[x + 9][y] &&
							_Data.Matrix[x + 10][y]))
					{
						score3 += 40;
					}
				}
				
				//Penalty 4
				double blackModules = 0;
				foreach (BitArray row in _Data.Matrix)
				foreach (bool bit in row)
				{
					if (bit)
						blackModules++;
				}
				
				double percent         = (blackModules / (_Data.Matrix.Count * _Data.Matrix.Count)) * 100;
				int prevMultipleOf5 = Math.Abs((int)Math.Floor(percent / 5) * 5 - 50) / 5;
				int nextMultipleOf5 = Math.Abs((int)Math.Floor(percent / 5) * 5 - 45) / 5;
				score4 = Math.Min(prevMultipleOf5, nextMultipleOf5) * 10;
				
				return score1 + score2 + score3 + score4;
			}
		}
	}

	static List<string> CalculateCorrectionWords(string _BITString, ErrorCorrectionInfo _ErrorCorrectionInfo)
	{
		int     correctionPerBlock = _ErrorCorrectionInfo.CorrectionPerBlock;
		Polynom messagePolynom     = CalculateMessagePolynom(_BITString);
		Polynom generatorPolynom   = CalculateGeneratorPolynom(correctionPerBlock);
		
		for (int i = 0; i < messagePolynom.Items.Count; i++)
		{
			messagePolynom.Items[i] = new PolynomItem(
				messagePolynom.Items[i].Coefficient,
				messagePolynom.Items[i].Exponent + correctionPerBlock
			);
		}
		
		for (int i = 0; i < generatorPolynom.Items.Count; i++)
		{
			generatorPolynom.Items[i] = new PolynomItem(
				generatorPolynom.Items[i].Coefficient,
				generatorPolynom.Items[i].Exponent + (messagePolynom.Items.Count - 1)
			);
		}
		
		Polynom leadTermSource = messagePolynom;
		for (int i = 0; (leadTermSource.Items.Count > 0 && leadTermSource.Items[leadTermSource.Items.Count - 1].Exponent > 0); i++)
		{
			if (leadTermSource.Items[0].Coefficient == 0)
			{
				leadTermSource.Items.RemoveAt(0);
				leadTermSource.Items.Add(new PolynomItem(0, leadTermSource.Items[leadTermSource.Items.Count - 1].Exponent - 1));
			}
			else
			{
				Polynom resultPolynom = MultiplyGeneratorPolynomByLeadTerm(generatorPolynom, ConvertToAlphaNotation(leadTermSource).Items[0], i);
				resultPolynom        = ConvertToDecNotation(resultPolynom);
				resultPolynom        = XORPolynoms(leadTermSource, resultPolynom);
				leadTermSource = resultPolynom;
			}
		}
		return leadTermSource.Items.Select(_Item => GetBinary(_Item.Coefficient, 8)).ToList();
	}

	static Polynom ConvertToAlphaNotation(Polynom _Polynom)
	{
		Polynom polynom = new Polynom();
		for (int i = 0; i < _Polynom.Items.Count; i++)
		{
			polynom.Items.Add(
				new PolynomItem(
					_Polynom.Items[i].Coefficient != 0
						? GetAlphaExpFromIntVal(_Polynom.Items[i].Coefficient)
						: 0,
					_Polynom.Items[i].Exponent
				)
			);
		}
		return polynom;
	}

	static Polynom ConvertToDecNotation(Polynom _Polynom)
	{
		var newPoly = new Polynom();
		for (var i = 0; i < _Polynom.Items.Count; i++)
			newPoly.Items.Add(new PolynomItem(GetIntValFromAlphaExp(_Polynom.Items[i].Coefficient), _Polynom.Items[i].Exponent));
		return newPoly;
	}

	static int GetVersion(int _Length, EncodingMode _EncodingMode, ErrorCorrection _ErrorCorrection)
	{
		var fittingVersions = m_CapacityTable.Where(
				_Entry => _Entry.Details.Any(
					_Details => _Details.ErrorCorrection == _ErrorCorrection && _Details.Capacity[_EncodingMode] >= Convert.ToInt32(_Length)
				)
			)
			.Select(
				_VersionInfo => new
				{
					version = _VersionInfo.Version,
					capacity = _VersionInfo.Details
						.Single(_Details => _Details.ErrorCorrection == _ErrorCorrection)
						.Capacity[_EncodingMode]
				}
			)
			.ToArray();
		
		if (fittingVersions.Any())
			return fittingVersions.Min(_Entry => _Entry.version);
		
		int maxSizeByte = m_CapacityTable.Where(
				_Entry => _Entry.Details.Any(
					_Details => _Details.ErrorCorrection == _ErrorCorrection
				)
			)
			.Max(_VersionInfo => _VersionInfo.Details.Single(_Details => _Details.ErrorCorrection == _ErrorCorrection).Capacity[_EncodingMode]);
		
		throw new QRCodeException(_ErrorCorrection.ToString(), _EncodingMode.ToString(), maxSizeByte);
	}

	static EncodingMode GetEncodingFromPlaintext(string _PlainText, bool _ForceUTF8)
	{
		if (_ForceUTF8)
			return EncodingMode.Byte;
		
		EncodingMode result = EncodingMode.Numeric;
		foreach (char symbol in _PlainText)
		{
			if (IsInRange(symbol, '0', '9'))
				continue;
			
			result = EncodingMode.Alphanumeric;
			
			if (IsInRange(symbol, 'A', 'Z') || m_AlphanumEncTable.Contains(symbol))
				continue;
			
			return EncodingMode.Byte;
		}
		return result;
	}

	static bool IsInRange(char _Symbol, char _Min, char _Max) => (uint)(_Symbol - _Min) <= (uint)(_Max - _Min);

	static Polynom CalculateMessagePolynom(string _BitString)
	{
		Polynom polynom = new Polynom();
		for (int i = _BitString.Length / 8 - 1; i >= 0; i--)
		{
			polynom.Items.Add(new PolynomItem(GetDecimal(_BitString.Substring(0, 8)), i));
			_BitString = _BitString.Remove(0, 8);
		}
		return polynom;
	}

	static Polynom CalculateGeneratorPolynom(int _CorrectionWords)
	{
		Polynom generatorPolynom = new Polynom();
		generatorPolynom.Items.AddRange(
			new[]
			{
				new PolynomItem(0, 1),
				new PolynomItem(0, 0)
			}
		);
		for (int i = 1; i <= _CorrectionWords - 1; i++)
		{
			var multiplierPolynom = new Polynom();
			multiplierPolynom.Items.AddRange(
				new[]
				{
					new PolynomItem(0, 1),
					new PolynomItem(i, 0)
				}
			);

			generatorPolynom = MultiplyAlphaPolynoms(generatorPolynom, multiplierPolynom);
		}

		return generatorPolynom;
	}

	static List<string> BinaryStringToBitBlockList(string _BitString)
	{
		const int    size  = 8;
		int          count = (int)Math.Ceiling(_BitString.Length / (double)size);
		List<string> block = new List<string>(count);
		
		for (int i = 0; i < _BitString.Length; i += size)
			block.Add(_BitString.Substring(i, size));
		
		return block;
	}

	static int GetDecimal(string _Value) => Convert.ToInt32(_Value, 2);

	static string GetBinary(int _Value) => Convert.ToString(_Value, 2);

	static string GetBinary(int _Value, int _Capacity) => GetBinary(_Value).PadLeft(_Capacity, '0');

	static int GetCountIndicatorLength(int _Version, EncodingMode _EncodingMode)
	{
		if (_Version < 10)
		{
			switch (_EncodingMode)
			{
				case EncodingMode.Numeric:
					return 10;
				case EncodingMode.Alphanumeric:
					return 9;
				default:
					return 8;
			}
		}
		else if (_Version < 27)
		{
			switch (_EncodingMode)
			{
				case EncodingMode.Numeric:
					return 12;
				case EncodingMode.Alphanumeric:
					return 11;
				case EncodingMode.Byte:
					return 16;
				default:
					return 10;
			}
		}
		else
		{
			switch (_EncodingMode)
			{
				case EncodingMode.Numeric:
					return 14;
				case EncodingMode.Alphanumeric:
					return 13;
				case EncodingMode.Byte:
					return 16;
				default:
					return 12;
			}
		}
	}

	static int GetDataLength(EncodingMode _EncodingMode, string _PlainText, string _CodedText, bool _ForceUTF8)
	{
		return _ForceUTF8 || CheckUTF8(_EncodingMode, _PlainText, false) ? (_CodedText.Length / 8) : _PlainText.Length;
	}

	static bool CheckUTF8(EncodingMode _EncodingMode, string _PlainText, bool _ForceUTF8)
	{
		return _EncodingMode == EncodingMode.Byte && (!CheckISO(_PlainText) || _ForceUTF8);
	}

	static bool CheckISO(string _Input)
	{
		Encoding encoding = Encoding.GetEncoding("ISO-8859-1"); 
		
		byte[] bytes = encoding.GetBytes(_Input);
		
		string result = encoding.GetString(bytes, 0, bytes.Length);
		
		return string.Equals(_Input, result);
	}

	static string PlainTextToBinary(string _PlainText, EncodingMode _EncodingMode, bool _ForceUTF8)
	{
		switch (_EncodingMode)
		{
			case EncodingMode.Alphanumeric:
				return PlainTextToBinaryAlphanumeric(_PlainText);
			case EncodingMode.Numeric:
				return PlainTextToBinaryNumeric(_PlainText);
			case EncodingMode.Byte:
				return PlainTextToBinaryByte(_PlainText, _ForceUTF8);
			case EncodingMode.Kanji:
				return string.Empty;
			case EncodingMode.ECI:
			default:
				return string.Empty;
		}
	}

	static string PlainTextToBinaryNumeric(string _PlainText)
	{
		string codeText = string.Empty;
		while (_PlainText.Length >= 3)
		{
			var dec = Convert.ToInt32(_PlainText.Substring(0, 3));
			codeText  += GetBinary(dec, 10);
			_PlainText =  _PlainText.Substring(3);
		}
		if (_PlainText.Length == 2)
		{
			int number = Convert.ToInt32(_PlainText);
			codeText += GetBinary(number, 7);
		}
		else if (_PlainText.Length == 1)
		{
			int number = Convert.ToInt32(_PlainText);
			codeText += GetBinary(number, 4);
		}
		return codeText;
	}

	static string PlainTextToBinaryAlphanumeric(string _PlainText)
	{
		string codeText = string.Empty;
		while (_PlainText.Length >= 2)
		{
			string token  = _PlainText.Substring(0, 2);
			int    number = m_AlphanumEncDict[token[0]] * 45 + m_AlphanumEncDict[token[1]];
			codeText  += GetBinary(number, 11);
			_PlainText =  _PlainText.Substring(2);
		}
		if (_PlainText.Length > 0)
			codeText += GetBinary(m_AlphanumEncDict[_PlainText[0]], 6);
		return codeText;
	}

	static string PlainTextToBinaryByte(string _PlainText, bool _ForceUTF8)
	{
		byte[] codeBytes;
		string codeText = string.Empty;
		
		if (CheckISO(_PlainText) && !_ForceUTF8)
			codeBytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(_PlainText);
		else
			codeBytes = Encoding.UTF8.GetBytes(_PlainText);
		
		return codeBytes.Aggregate(codeText, (_A, _B) => _A + GetBinary(_B, 8));
	}

	static Polynom XORPolynoms(Polynom _MessagePolynom, Polynom _ResultPolynom)
	{
		Polynom resultPolynom = new Polynom();
		Polynom longPolynom;
		Polynom shortPolynom;
		
		if (_MessagePolynom.Items.Count >= _ResultPolynom.Items.Count)
		{
			longPolynom  = _MessagePolynom;
			shortPolynom = _ResultPolynom;
		}
		else
		{
			longPolynom  = _ResultPolynom;
			shortPolynom = _MessagePolynom;
		}
		
		for (int i = 0; i < longPolynom.Items.Count; i++)
		{
			PolynomItem item = new PolynomItem
			(
				longPolynom.Items[i].Coefficient ^
				(shortPolynom.Items.Count > i ? shortPolynom.Items[i].Coefficient : 0),
				_MessagePolynom.Items[0].Exponent - i
			);
			resultPolynom.Items.Add(item);
		}
		resultPolynom.Items.RemoveAt(0);
		return resultPolynom;
	}

	static Polynom MultiplyGeneratorPolynomByLeadTerm(Polynom _GeneratePolynom, PolynomItem _LeadTerm, int _LowerExponent)
	{
		Polynom resultPolynom = new Polynom();
		foreach (PolynomItem item in _GeneratePolynom.Items)
		{
			resultPolynom.Items.Add(new PolynomItem(
				(item.Coefficient + _LeadTerm.Coefficient) % 255,
				item.Exponent - _LowerExponent
			));
		}
		return resultPolynom;
	}

	static Polynom MultiplyAlphaPolynoms(Polynom _BasePolynom, Polynom _MultiplierPolynom)
	{
		Polynom resultPolynom = new Polynom();
		foreach (PolynomItem multiplierItem in _MultiplierPolynom.Items)
		foreach (PolynomItem baseItem in _BasePolynom.Items)
		{
			PolynomItem item = new PolynomItem
			(
				ShrinkAlphaExp(multiplierItem.Coefficient + baseItem.Coefficient),
				multiplierItem.Exponent + baseItem.Exponent
			);
			resultPolynom.Items.Add(item);
		}
		
		IEnumerable<int>  exponentsToGlue = resultPolynom.Items
			.GroupBy(_Item => _Item.Exponent)
			.Where(_Item => _Item.Count() > 1)
			.Select(_Item => _Item.First().Exponent);
		
		IList<int>        toGlue          = exponentsToGlue as IList<int> ?? exponentsToGlue.ToList();
		List<PolynomItem> gluedPolynoms   = new List<PolynomItem>(toGlue.Count);
		foreach (int exponent in toGlue)
		{
			int coefficient = resultPolynom.Items
				.Where(_Item => _Item.Exponent == exponent)
				.Aggregate(0, (_A, _B) => _A ^ GetIntValFromAlphaExp(_B.Coefficient));
			PolynomItem polynomFixed = new PolynomItem(GetAlphaExpFromIntVal(coefficient), exponent);
			gluedPolynoms.Add(polynomFixed);
		}
		resultPolynom.Items.RemoveAll(_Item => toGlue.Contains(_Item.Exponent));
		resultPolynom.Items.AddRange(gluedPolynoms);
		resultPolynom.Items.Sort((_A, _B) => -_A.Exponent.CompareTo(_B.Exponent));
		return resultPolynom;
	}

	static int GetIntValFromAlphaExp(int _Exp)
	{
		return m_GaloisField.Find(_ALog => _ALog.ExponentAlpha == _Exp).IntegerValue;
	}

	static int GetAlphaExpFromIntVal(int _IntVal)
	{
		return m_GaloisField.Find(_ALog => _ALog.IntegerValue == _IntVal).ExponentAlpha;
	}

	static int ShrinkAlphaExp(int _AlphaExp)
	{
		// ReSharper disable once PossibleLossOfFraction
		return (int)(_AlphaExp % 256 + Math.Floor((double)(_AlphaExp / 256)));
	}

	static Dictionary<char, int> CreateAlphanumEncDict()
	{
		Dictionary<char, int> localAlphanumEncDict = new Dictionary<char, int>(45);
		//Add numbers
		for (int i = 0; i < 10; i++)
			localAlphanumEncDict.Add($"{i}"[0], i);
		//Add chars
		for (char c = 'A'; c <= 'Z'; c++)
			localAlphanumEncDict.Add(c, localAlphanumEncDict.Count());
		//Add special chars
		for (int i = 0; i < m_AlphanumEncTable.Length; i++)
			localAlphanumEncDict.Add(m_AlphanumEncTable[i], localAlphanumEncDict.Count());
		return localAlphanumEncDict;
	}

	static List<AlignmentPattern> CreateAlignmentPatternTable()
	{
		List<AlignmentPattern> localAlignmentPatternTable = new List<AlignmentPattern>(40);
		for (int i = 0; i < 7 * 40; i += 7)
		{
			List<Vector2Int> points = new List<Vector2Int>();
			for (int x = 0; x < 7; x++)
			{
				if (m_AlignmentPatternBaseValues[i + x] == 0)
					continue;
				
				for (var y = 0; y < 7; y++)
				{
					if (m_AlignmentPatternBaseValues[i + y] == 0)
						continue;
					
					Vector2Int point = new Vector2Int(m_AlignmentPatternBaseValues[i + x] - 2, m_AlignmentPatternBaseValues[i + y] - 2);
					if (!points.Contains(point))
						points.Add(point);
				}
			}
			
			localAlignmentPatternTable.Add(
				new AlignmentPattern()
				{
					Version          = (i + 7) / 7,
					PatternPositions = points
				}
			);
		}
		return localAlignmentPatternTable;
	}


	static List<ErrorCorrectionInfo> CreateCorrectionTable()
	{
		List<ErrorCorrectionInfo> correctionTable = new List<ErrorCorrectionInfo>(160);
		for (var i = 0; i < 4 * 6 * 40; i += 24)
		{
			correctionTable.AddRange(
				new[]
				{
					new ErrorCorrectionInfo(
						(i + 24) / 24,
						ErrorCorrection.L,
						m_CapacityEccBaseValues[i],
						m_CapacityEccBaseValues[i + 1],
						m_CapacityEccBaseValues[i + 2],
						m_CapacityEccBaseValues[i + 3],
						m_CapacityEccBaseValues[i + 4],
						m_CapacityEccBaseValues[i + 5]
					),
					new ErrorCorrectionInfo
					(
						_Version: (i + 24) / 24,
						_ErrorCorrection: ErrorCorrection.M,
						_TotalDataCodewords: m_CapacityEccBaseValues[i + 6],
						_CorrectionPerBlock: m_CapacityEccBaseValues[i + 7],
						_BlocksInGroup1: m_CapacityEccBaseValues[i + 8],
						_CodewordsInGroup1: m_CapacityEccBaseValues[i + 9],
						_BlocksInGroup2: m_CapacityEccBaseValues[i + 10],
						_CodewordsInGroup2: m_CapacityEccBaseValues[i + 11]
					),
					new ErrorCorrectionInfo
					(
						_Version: (i + 24) / 24,
						_ErrorCorrection: ErrorCorrection.Q,
						_TotalDataCodewords: m_CapacityEccBaseValues[i + 12],
						_CorrectionPerBlock: m_CapacityEccBaseValues[i + 13],
						_BlocksInGroup1: m_CapacityEccBaseValues[i + 14],
						_CodewordsInGroup1: m_CapacityEccBaseValues[i + 15],
						_BlocksInGroup2: m_CapacityEccBaseValues[i + 16],
						_CodewordsInGroup2: m_CapacityEccBaseValues[i + 17]
					),
					new ErrorCorrectionInfo
					(
						_Version: (i + 24) / 24,
						_ErrorCorrection: ErrorCorrection.H,
						_TotalDataCodewords: m_CapacityEccBaseValues[i + 18],
						_CorrectionPerBlock: m_CapacityEccBaseValues[i + 19],
						_BlocksInGroup1: m_CapacityEccBaseValues[i + 20],
						_CodewordsInGroup1: m_CapacityEccBaseValues[i + 21],
						_BlocksInGroup2: m_CapacityEccBaseValues[i + 22],
						_CodewordsInGroup2: m_CapacityEccBaseValues[i + 23]
					)
				}
			);
		}
		return correctionTable;
	}

	static List<VersionInfo> CreateCapacityTable()
	{
		List<VersionInfo> capacityTable = new List<VersionInfo>(40);
		for (var i = 0; i < 640; i += 16)
		{
			capacityTable.Add(
				new VersionInfo(
					(i + 16) / 16,
					new List<VersionInfoDetails>(4)
					{
						new VersionInfoDetails(
							ErrorCorrection.L,
							new Dictionary<EncodingMode, int>()
							{
								{ EncodingMode.Numeric, m_CapacityBaseValues[i] },
								{ EncodingMode.Alphanumeric, m_CapacityBaseValues[i + 1] },
								{ EncodingMode.Byte, m_CapacityBaseValues[i + 2] },
								{ EncodingMode.Kanji, m_CapacityBaseValues[i + 3] },
							}
						),
						new VersionInfoDetails(
							ErrorCorrection.M,
							new Dictionary<EncodingMode, int>()
							{
								{ EncodingMode.Numeric, m_CapacityBaseValues[i + 4] },
								{ EncodingMode.Alphanumeric, m_CapacityBaseValues[i + 5] },
								{ EncodingMode.Byte, m_CapacityBaseValues[i + 6] },
								{ EncodingMode.Kanji, m_CapacityBaseValues[i + 7] },
							}
						),
						new VersionInfoDetails(
							ErrorCorrection.Q,
							new Dictionary<EncodingMode, int>()
							{
								{ EncodingMode.Numeric, m_CapacityBaseValues[i + 8] },
								{ EncodingMode.Alphanumeric, m_CapacityBaseValues[i + 9] },
								{ EncodingMode.Byte, m_CapacityBaseValues[i + 10] },
								{ EncodingMode.Kanji, m_CapacityBaseValues[i + 11] },
							}
						),
						new VersionInfoDetails(
							ErrorCorrection.H,
							new Dictionary<EncodingMode, int>()
							{
								{ EncodingMode.Numeric, m_CapacityBaseValues[i + 12] },
								{ EncodingMode.Alphanumeric, m_CapacityBaseValues[i + 13] },
								{ EncodingMode.Byte, m_CapacityBaseValues[i + 14] },
								{ EncodingMode.Kanji, m_CapacityBaseValues[i + 15] },
							}
						)
					}
				)
			);
		}
		return capacityTable;
	}

	static List<Antilog> CreateAntilogTable()
	{
		var localGaloisField = new List<Antilog>(256);

		int gfItem = 1;
		for (var i = 0; i < 256; i++)
		{
			localGaloisField.Add(new Antilog(i, gfItem));
			gfItem *= 2;
			if (gfItem > 255)
				gfItem ^= 285;
		}
		return localGaloisField;
	}

	struct AlignmentPattern
	{
		public int              Version;
		public List<Vector2Int> PatternPositions;
	}

	struct CodewordBlock
	{
		public List<string> CodeWords       { get; }
		public List<string> CorrectionWords { get; }

		public CodewordBlock(
			List<string> _CodeWords,
			List<string> _CorrectionWords
		)
		{
			CodeWords       = _CodeWords;
			CorrectionWords = _CorrectionWords;
		}
	}

	struct ErrorCorrectionInfo
	{
		public int             Version            { get; }
		public ErrorCorrection ErrorCorrection    { get; }
		public int             TotalDataCodewords { get; }
		public int             CorrectionPerBlock { get; }
		public int             BlocksInGroup1     { get; }
		public int             CodewordsInGroup1  { get; }
		public int             BlocksInGroup2     { get; }
		public int             CodewordsInGroup2  { get; }

		public ErrorCorrectionInfo(
			int             _Version,
			ErrorCorrection _ErrorCorrection,
			int             _TotalDataCodewords,
			int             _CorrectionPerBlock,
			int             _BlocksInGroup1,
			int             _CodewordsInGroup1,
			int             _BlocksInGroup2,
			int             _CodewordsInGroup2
		)
		{
			Version            = _Version;
			ErrorCorrection    = _ErrorCorrection;
			TotalDataCodewords = _TotalDataCodewords;
			CorrectionPerBlock = _CorrectionPerBlock;
			BlocksInGroup1     = _BlocksInGroup1;
			CodewordsInGroup1  = _CodewordsInGroup1;
			BlocksInGroup2     = _BlocksInGroup2;
			CodewordsInGroup2  = _CodewordsInGroup2;
		}
	}

	struct VersionInfo
	{
		public int                      Version { get; }
		public List<VersionInfoDetails> Details { get; }

		public VersionInfo(int _Version, List<VersionInfoDetails> _VersionInfoDetails)
		{
			Version = _Version;
			Details = _VersionInfoDetails;
		}
	}

	struct VersionInfoDetails
	{
		public ErrorCorrection               ErrorCorrection { get; }
		public Dictionary<EncodingMode, int> Capacity        { get; }

		public VersionInfoDetails(ErrorCorrection _ErrorCorrection, Dictionary<EncodingMode, int> _Capacity)
		{
			ErrorCorrection = _ErrorCorrection;
			Capacity        = _Capacity;
		}
	}

	struct Antilog
	{
		public int ExponentAlpha { get; }
		public int IntegerValue  { get; }

		public Antilog(int _ExponentAlpha, int _IntegerValue)
		{
			ExponentAlpha = _ExponentAlpha;
			IntegerValue  = _IntegerValue;
		}
	}

	struct PolynomItem
	{
		public int Coefficient { get; }
		public int Exponent    { get; }

		public PolynomItem(int _Coefficient, int _Exponent)
		{
			Coefficient = _Coefficient;
			Exponent    = _Exponent;
		}
	}

	class Polynom
	{
		public List<PolynomItem> Items { get; }

		public Polynom()
		{
			Items = new List<PolynomItem>();
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			foreach (PolynomItem item in Items)
				builder.AppendFormat("a^{0}*x^{1} + ", item.Coefficient, item.Exponent);
			return builder.ToString().TrimEnd(' ', '+');
		}
	}
}