using System;
using OggVorbisEncoder.Setup;

namespace OggVorbisEncoder
{
	public static class HeaderPacketBuilder
	{
		const string VorbisString = "vorbis";
		const string VendorString = "OggVorbisEncoder";

		public static OggPacket BuildInfoPacket(VorbisInfo _VorbisInfo)
		{
			var buffer = new EncodeBuffer();

			PackInfo(buffer, _VorbisInfo);

			var bytes = buffer.GetBytes();
			return new OggPacket(bytes, false, 0, 0);
		}

		public static OggPacket BuildCommentsPacket(Comments _Comments)
		{
			var buffer = new EncodeBuffer();

			PackComment(buffer, _Comments);

			var bytes = buffer.GetBytes();
			return new OggPacket(bytes, false, 0, 1);
		}

		public static OggPacket BuildBooksPacket(VorbisInfo _VorbisInfo)
		{
			var buffer = new EncodeBuffer(4096);

			PackBooks(buffer, _VorbisInfo);

			var bytes = buffer.GetBytes();
			return new OggPacket(bytes, false, 0, 2);
		}

		static void PackBooks(EncodeBuffer _Buffer, VorbisInfo _VorbisInfo)
		{
			var codecSetup = _VorbisInfo.CodecSetup;

			_Buffer.Write(0x05, 8);
			_Buffer.WriteString(VorbisString);

			_Buffer.Write((uint)(codecSetup.BookParams.Count - 1), 8);
			foreach (var book in codecSetup.BookParams)
				PackStaticBook(_Buffer, book);

			// times; hook placeholders 
			_Buffer.Write(0, 6);
			_Buffer.Write(0, 16);

			_Buffer.Write((uint)(codecSetup.FloorParams.Count - 1), 6);
			foreach (var floor in codecSetup.FloorParams)
			{
				_Buffer.Write(1, 16); // For now we're only using floor type 1
				PackFloor(_Buffer, floor);
			}

			_Buffer.Write((uint)(codecSetup.ResidueParams.Count - 1), 6);
			foreach (var residue in codecSetup.ResidueParams)
			{
				_Buffer.Write((uint)residue.ResidueType, 16);
				PackResidue(_Buffer, residue);
			}

			_Buffer.Write((uint)(codecSetup.MapParams.Count - 1), 6);
			foreach (var mapping in codecSetup.MapParams)
			{
				_Buffer.Write(0, 16); // Mapping type is always zero
				PackMapping(_Buffer, _VorbisInfo, mapping);
			}

			_Buffer.Write((uint)(codecSetup.ModeParams.Count - 1), 6);
			for (var i = 0; i < codecSetup.ModeParams.Count; i++)
				PackModes(_Buffer, codecSetup, i);

			_Buffer.Write(1, 1);
		}

		static void PackModes(EncodeBuffer _Buffer, CodecSetup _CodecSetup, int _Index)
		{
			_Buffer.Write((uint)_CodecSetup.ModeParams[_Index].BlockFlag, 1);
			_Buffer.Write((uint)_CodecSetup.ModeParams[_Index].WindowType, 16);
			_Buffer.Write((uint)_CodecSetup.ModeParams[_Index].TransformType, 16);
			_Buffer.Write((uint)_CodecSetup.ModeParams[_Index].Mapping, 8);
		}

		static void PackResidue(EncodeBuffer _Buffer, ResidueEntry _Residue)
		{
			_Buffer.Write((uint)_Residue.Begin, 24);
			_Buffer.Write((uint)_Residue.End, 24);

			_Buffer.Write((uint)(_Residue.Grouping - 1), 24);
			// residue vectors to group and code with a partitioned book 
			_Buffer.Write((uint)(_Residue.Partitions - 1), 6); // possible partition choices 
			_Buffer.Write((uint)_Residue.GroupBook, 8); // group huffman book 

			var acc = 0;

			// secondstages is a bitmask; as encoding progresses pass by pass, a
			// bitmask of one indicates this partition class has bits to write this pass 
			for (var j = 0; j < _Residue.Partitions; j++)
			{
				if (Encoding.Log(_Residue.SecondStages[j]) > 3)
				{
					// yes, this is a minor hack due to not thinking ahead 
					_Buffer.Write((uint)_Residue.SecondStages[j], 3);
					_Buffer.Write(1, 1);
					_Buffer.Write((uint)_Residue.SecondStages[j] >> 3, 5);
				}
				else
				{
					_Buffer.Write((uint)_Residue.SecondStages[j], 4); // trailing zero 
				}

				acc += Count(_Residue.SecondStages[j]);
			}

			for (var j = 0; j < acc; j++)
				_Buffer.Write((uint)_Residue.BookList[j], 8);
		}

		static int Count(int _Value)
		{
			var ret = 0;
			while (_Value != 0)
			{
				ret    +=  _Value & 1;
				_Value >>= 1;
			}
			return ret;
		}

		static void PackFloor(EncodeBuffer _Buffer, Floor _Floor)
		{
			var count    = 0;
			var maxposit = _Floor.PostList[1];
			var maxclass = -1;

			// save out partitions 
			_Buffer.Write((uint)_Floor.PartitionClass.Length, 5); // only 0 to 31 legal 
			foreach (var partitionClass in _Floor.PartitionClass)
			{
				_Buffer.Write((uint)partitionClass, 4); // only 0 to 15 legal 
				if (maxclass < partitionClass)
					maxclass = partitionClass;
			}

			// save out partition classes 
			for (var j = 0; j < maxclass + 1; j++)
			{
				_Buffer.Write((uint)(_Floor.ClassDimensions[j] - 1), 3); // 1 to 8 
				_Buffer.Write((uint)_Floor.ClassSubs[j], 2); // 0 to 3 
				if (_Floor.ClassSubs[j] != 0)
					_Buffer.Write((uint)_Floor.ClassBook[j], 8);

				for (var k = 0; k < 1 << _Floor.ClassSubs[j]; k++)
					_Buffer.Write((uint)(_Floor.ClassSubBook[j][k] + 1), 8);
			}

			// save out the post list, only 1,2,3,4 legal now 
			_Buffer.Write((uint)(_Floor.Mult - 1), 2);

			// maxposit cannot legally be less than 1; this is encode-side, we can assume our setup is OK 
			_Buffer.Write((uint)Encoding.Log(maxposit - 1), 4);
			var rangebits = Encoding.Log(maxposit - 1);

			for (int j = 0, k = 0; j < _Floor.PartitionClass.Length; j++)
			{
				count += _Floor.ClassDimensions[_Floor.PartitionClass[j]];
				for (; k < count; k++)
					_Buffer.Write((uint)_Floor.PostList[k + 2], rangebits);
			}
		}

		static void PackMapping(EncodeBuffer _Buffer, VorbisInfo _Info, Mapping _Mapping)
		{
			/* another 'we meant to do it this way' hack...  up to beta 4, we
				packed 4 binary zeros here to signify one submapping in use.  We
				now redefine that to mean four bitflags that indicate use of
				deeper features; bit0:submappings, bit1:coupling,
				bit2,3:reserved. This is backward compatible with all actual uses
				of the beta code. */
			if (_Mapping.SubMaps > 1)
			{
				_Buffer.Write(1, 1);
				_Buffer.Write((uint)_Mapping.SubMaps - 1, 4);
			}
			else
			{
				_Buffer.Write(0, 1);
			}

			if (_Mapping.CouplingSteps > 0)
			{
				_Buffer.Write(1, 1);
				_Buffer.Write((uint)_Mapping.CouplingSteps - 1, 8);

				var couplingBits = Encoding.Log(_Info.Channels - 1);
				for (var i = 0; i < _Mapping.CouplingSteps; i++)
				{
					_Buffer.Write((uint)_Mapping.CouplingMag[i], couplingBits);
					_Buffer.Write((uint)_Mapping.CouplingAng[i], couplingBits);
				}
			}
			else
			{
				_Buffer.Write(0, 1);
			}

			_Buffer.Write(0, 2); // 2,3:reserved 

			// we don't write the channel submappings if we only have one... 
			if (_Mapping.SubMaps > 1)
				for (var i = 0; i < _Info.Channels; i++)
					_Buffer.Write((uint)_Mapping.ChannelMuxList[i], 4);

			for (var i = 0; i < _Mapping.SubMaps; i++)
			{
				_Buffer.Write(0, 8); // time submap unused 
				_Buffer.Write((uint)_Mapping.FloorSubMap[i], 8);
				_Buffer.Write((uint)_Mapping.ResidueSubMap[i], 8);
			}
		}

		static void PackStaticBook(EncodeBuffer _Buffer, IStaticCodeBook _Book)
		{
			var ordered = false;

			// first the basic parameters
			_Buffer.Write(0x564342, 24);
			_Buffer.Write((uint)_Book.Dimensions, 16);
			_Buffer.Write((uint)_Book.LengthList.Length, 24);

			// pack the codewords.  There are two packing types; length ordered and length random. 
			int i;
			for (i = 1; i < _Book.LengthList.Length; i++)
				if (_Book.LengthList[i - 1] == 0 || _Book.LengthList[i] < _Book.LengthList[i - 1])
					break;

			if (i == _Book.LengthList.Length)
				ordered = true;

			if (ordered)
			{
				// length ordered.  We only need to say how many codewords of each length.  The actual codewords are generated deterministically 
				_Buffer.Write(1, 1);

				_Buffer.Write((uint)(_Book.LengthList[0] - 1), 5); // 1 to 32

				var count = 0;
				for (i = 1; i < _Book.LengthList.Length; i++)
				{
					var current  = _Book.LengthList[i];
					var previous = _Book.LengthList[i - 1];

					if (current <= previous)
						continue;

					for (var j = previous; j < current; j++)
					{
						_Buffer.Write((uint)(i - count), Encoding.Log(_Book.LengthList.Length - count));
						count = i;
					}
				}

				_Buffer.Write((uint)(i - count), Encoding.Log(_Book.LengthList.Length - count));
			}
			else
			{
				// length unordered. Again, we don't code the codeword itself, just the length. This time, though, we have to encode each length 
				_Buffer.Write(0, 1);

				/* algorithmic mapping has use for 'unused entries', which we tag
					here.  The algorithmic mapping happens as usual, but the unused
					entry has no codeword. */
				for (i = 0; i < _Book.LengthList.Length; i++)
					if (_Book.LengthList[i] == 0)
						break;

				if (i == _Book.LengthList.Length)
				{
					_Buffer.Write(0, 1); // no unused entries
					for (i = 0; i < _Book.LengthList.Length; i++)
						_Buffer.Write((uint)(_Book.LengthList[i] - 1), 5);
				}
				else
				{
					_Buffer.Write(1, 1); // we have unused entries; thus we tag 
					for (i = 0; i < _Book.LengthList.Length; i++)
						if (_Book.LengthList[i] == 0)
						{
							_Buffer.Write(0, 1);
						}
						else
						{
							_Buffer.Write(1, 1);
							_Buffer.Write((uint)(_Book.LengthList[i] - 1), 5);
						}
				}
			}

			_Buffer.Write((uint)_Book.MapType, 4);
			if (_Book.MapType == CodeBookMapType.None)
				return;

			// is the entry number the desired return value, or do we have a mapping? If we have a mapping, what type? 
			if (_Book.MapType != CodeBookMapType.Implicit && _Book.MapType != CodeBookMapType.Listed)
				throw new InvalidOperationException($"Unknown {nameof(CodeBookMapType)}: {_Book.MapType}");

			if (_Book.QuantList == null)
				throw new InvalidOperationException($"{nameof(_Book.QuantList)} cannot be null");

			// values that define the dequantization 
			_Buffer.Write((uint)_Book.QuantMin, 32);
			_Buffer.Write((uint)_Book.QuantDelta, 32);
			_Buffer.Write((uint)(_Book.Quant - 1), 4);
			_Buffer.Write((uint)_Book.QuantSequenceP, 1);

			var quantVals = 0;
			switch (_Book.MapType)
			{
				case CodeBookMapType.Implicit:
					// a single column of (c.entries/c.dim) quantized values for building a full value list algorithmically (square lattice) 
					quantVals = _Book.GetQuantVals();
					break;
				case CodeBookMapType.Listed:
					// every value (c.entries*c.dim total) specified explicitly 
					quantVals = _Book.LengthList.Length * _Book.Dimensions;
					break;
			}

			// quantized values 
			for (i = 0; i < quantVals; i++)
				_Buffer.Write((uint)Math.Abs(_Book.QuantList[i]), _Book.Quant);
		}

		static void PackComment(EncodeBuffer _Buffer, Comments _Comments)
		{
			// Preamble
			_Buffer.Write(0x03, 8);
			_Buffer.WriteString(VorbisString);

			// Vendor
			_Buffer.Write((uint)VendorString.Length, 32);
			_Buffer.WriteString(VendorString);

			// Comments
			_Buffer.Write((uint)_Comments.UserComments.Count, 32);

			foreach (var comment in _Comments.UserComments)
				if (!string.IsNullOrEmpty(comment))
				{
					_Buffer.Write((uint)comment.Length, 32);
					_Buffer.WriteString(comment);
				}
				else
				{
					_Buffer.Write(0, 32);
				}

			_Buffer.Write(1, 1);
		}

		static void PackInfo(EncodeBuffer _Buffer, VorbisInfo _VorbisInfo)
		{
			var codecSetup = _VorbisInfo.CodecSetup;

			// preamble
			_Buffer.Write(0x01, 8);
			_Buffer.WriteString(VorbisString);

			// basic information about the stream 
			_Buffer.Write(0x00, 32);
			_Buffer.Write((uint)_VorbisInfo.Channels, 8);
			_Buffer.Write((uint)_VorbisInfo.SampleRate, 32);

			_Buffer.Write(0, 32); // Bit rate upper not used
			_Buffer.Write((uint)_VorbisInfo.BitRateNominal, 32);
			_Buffer.Write(0, 32); // Bit rate lower not used

			_Buffer.Write((uint)Encoding.Log(codecSetup.BlockSizes[0] - 1), 4);
			_Buffer.Write((uint)Encoding.Log(codecSetup.BlockSizes[1] - 1), 4);
			_Buffer.Write(1, 1);
		}
	}
}