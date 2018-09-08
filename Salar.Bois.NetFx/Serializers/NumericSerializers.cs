﻿using System;
using System.IO;

namespace Salar.Bois.Serializers
{
	internal static class NumericSerializers
	{
		/// <summary>
		/// 0100 0000 = 64
		/// </summary>
		internal const byte FlagNullable = 0b0_1_0_0_0_0_0_0;

		/// <summary>
		/// 1000 0000 = 128
		/// </summary>
		private const byte FlagEmbdedded = 0b1_0_0_0_0_0_0_0;

		/// <summary>
		/// 127
		/// </summary>
		private const byte FlagEmbdeddedMask = 0b0_1_1_1_1_1_1_1;

		/// <summary>
		/// 0011 1111 = 63
		/// </summary>
		private const byte EmbeddedSignedMaxNumInByte = 0b0_0_1_1_1_1_1_1;// 63;

		/// <summary>
		/// 0100 0000 = 64
		/// </summary>
		private const byte FlagNonullNegativeNum = 0b0_1_0_0_0_0_0_0;

		/// <summary>
		/// 191
		/// </summary>
		private const byte FlagNonullNegativeMask = 0b1_0_1_1_1_1_1_1;

		/// <summary>
		/// 192
		/// </summary>
		private const byte FlagNonullNegativeNumEmbdedded = 0b1_1_0_0_0_0_0_0;

		/// <summary>
		/// 63
		/// </summary>
		private const byte FlagNonullNegativeNumEmbdeddedMask = 0b0_0_1_1_1_1_1_1;

		/// <summary>
		/// 32
		/// </summary>
		private const byte FlagNullableNegativeNum = 0b0_0_1_0_0_0_0_0;

		/// <summary>
		/// 223
		/// </summary>
		private const byte FlagNullableNegativeMask = 0b1_1_0_1_1_1_1_1;

		/// <summary>
		/// 160
		/// </summary>
		private const byte FlagNullableNegativeEmbdeddedNum = 0b1_0_1_0_0_0_0_0;

		/// <summary>
		/// 95
		/// </summary>
		private const byte FlagNullableNegativeEmbdeddedMask = 0b0_1_0_1_1_1_1_1;

		/// <summary>
		/// 127
		/// </summary>
		private const byte EmbeddedUnsignedMaxNumInByte = 0b0_1_1_1_1_1_1_1;

		/// <summary>
		/// 31
		/// </summary>
		private const byte EmbeddedSignedNullableMaxNumInByte = 0b0_0_0_1_1_1_1_1;

		/// <summary>
		/// 63
		/// </summary>
		private const byte EmbeddedUnsignedNullableMaxNumInByte = 0b0_0_1_1_1_1_1_1;

		#region Array Pool

		private static class SharedArray
		{
			[ThreadStatic] private static byte[] _array;

			public static byte[] Get()
			{
				return _array ?? (_array = new byte[16]);
			}

			public static void ClearArray4()
			{
				_array[0] = 0;
				_array[1] = 0;
				_array[2] = 0;
				_array[3] = 0;
			}
			public static void ClearArray8()
			{
				_array[0] = 0;
				_array[1] = 0;
				_array[2] = 0;
				_array[3] = 0;
				_array[4] = 0;
				_array[5] = 0;
				_array[6] = 0;
				_array[7] = 0;
			}
		}

		#endregion

		#region Readers


		internal static short? ReadVarInt16Nullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == FlagNullable)
				return null;

			var embedded = (input & FlagEmbdedded) == FlagEmbdedded;
			var negative = (input & FlagNullableNegativeNum) == FlagNullableNegativeNum;

			if (embedded)
			{
				if (negative)
				{
					var theNumber = input & FlagNullableNegativeEmbdeddedMask;

					return (short)-theNumber;
				}
				else
				{
					return (short)(input & FlagEmbdeddedMask);
				}
			}
			else
			{
				if (negative)
				{
					var length = input & FlagNullableNegativeMask;

					var numBuff = reader.ReadBytes(length);

					return (short)-ConvertFromVarBinaryInt16(numBuff);
				}
				else
				{
					var length = input;

					var numBuff = reader.ReadBytes(length);

					return ConvertFromVarBinaryInt16(numBuff);
				}
			}
		}

		internal static short ReadVarInt16(BinaryReader reader)
		{
			var input = reader.ReadByte();

			var embedded = (input & FlagEmbdedded) == FlagEmbdedded;
			var negative = (input & FlagNonullNegativeNum) == FlagNonullNegativeNum;

			if (embedded)
			{
				if (negative)
				{
					var thenumber = input & FlagNonullNegativeNumEmbdeddedMask;

					return (short)-thenumber;
				}
				else
				{
					return (short)(input & FlagEmbdeddedMask);
				}
			}
			else
			{
				if (negative)
				{
					var length = input & FlagNonullNegativeMask;

					var numBuff = reader.ReadBytes(length);

					return (short)-ConvertFromVarBinaryInt16(numBuff);
				}
				else
				{
					var length = input;

					var numBuff = reader.ReadBytes(length);

					return ConvertFromVarBinaryInt16(numBuff);
				}
			}
		}

		internal static ushort? ReadVarUInt16Nullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == FlagNullable)
				return null;

			var embedded = (input & FlagEmbdedded) == FlagEmbdedded;

			if (embedded)
			{
				return (ushort)(input & FlagEmbdeddedMask);
			}
			else
			{
				var length = input;

				var numBuff = reader.ReadBytes(length);

				return ConvertFromVarBinaryUInt16(numBuff);
			}
		}

		internal static ushort ReadVarUInt16(BinaryReader reader)
		{
			var input = reader.ReadByte();

			var embedded = (input & FlagEmbdedded) == FlagEmbdedded;

			if (embedded)
			{
				return (ushort)(input & FlagEmbdeddedMask);
			}
			else
			{
				var length = input;

				var numBuff = reader.ReadBytes(length);

				return ConvertFromVarBinaryUInt16(numBuff);
			}
		}

		internal static int? ReadVarInt32Nullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == FlagNullable)
				return null;

			var embedded = (input & FlagEmbdedded) == FlagEmbdedded;
			var negative = (input & FlagNullableNegativeNum) == FlagNullableNegativeNum;

			if (embedded)
			{
				if (negative)
				{
					var thenumber = input & FlagNullableNegativeEmbdeddedMask;

					return -thenumber;
				}
				else
				{
					return input & FlagEmbdeddedMask;
				}
			}
			else
			{
				var buff = SharedArray.Get();
				if (negative)
				{
					var length = input & FlagNullableNegativeMask;

					if (length < 4)
						SharedArray.ClearArray4();

					reader.Read(buff, 0, length);

					return -ConvertFromVarBinaryInt32(buff, length);
				}
				else
				{
					var length = input;

					if (length < 4)
						SharedArray.ClearArray4();

					reader.Read(buff, 0, length);

					return ConvertFromVarBinaryInt32(buff, length);
				}
			}
		}

		internal static int ReadVarInt32(BinaryReader reader)
		{
			var input = reader.ReadByte();

			var embedded = (input & FlagEmbdedded) == FlagEmbdedded;
			var negative = (input & FlagNonullNegativeNum) == FlagNonullNegativeNum;

			if (embedded)
			{
				if (negative)
				{
					var theNumber = input & FlagNonullNegativeNumEmbdeddedMask;

					return -theNumber;
				}
				else
				{
					return input & FlagEmbdeddedMask;
				}
			}
			else
			{
				var buff = SharedArray.Get();

				if (negative)
				{
					int length = input & FlagNonullNegativeMask;

					if (length < 4)
						SharedArray.ClearArray4();

					reader.Read(buff, 0, length);

					return -ConvertFromVarBinaryInt32(buff, length);
				}
				else
				{
					int length = input;

					if (length < 4)
						SharedArray.ClearArray4();

					reader.Read(buff, 0, length);

					return ConvertFromVarBinaryInt32(buff, length);
				}
			}
		}

		internal static uint? ReadVarUInt32Nullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == FlagNullable)
				return null;

			var embedded = (input & FlagEmbdedded) == FlagEmbdedded;

			if (embedded)
			{
				return (uint)input & FlagEmbdeddedMask;
			}
			else
			{
				var buff = SharedArray.Get();
				int length = input;

				if (length < 4)
					SharedArray.ClearArray4();

				reader.Read(buff, 0, length);

				return ConvertFromVarBinaryUInt32(buff, length);
			}
		}

		internal static uint ReadVarUInt32(BinaryReader reader)
		{
			var input = reader.ReadByte();

			var embedded = (input & FlagEmbdedded) == FlagEmbdedded;

			if (embedded)
			{
				return (uint)input & FlagEmbdeddedMask;
			}
			else
			{
				var buff = SharedArray.Get();
				int length = input;

				if (length < 4)
					SharedArray.ClearArray4();

				reader.Read(buff, 0, length);

				return ConvertFromVarBinaryUInt32(buff, length);
			}
		}

		internal static long? ReadVarInt64Nullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == FlagNullable)
				return null;

			var embedded = (input & FlagEmbdedded) == FlagEmbdedded;
			var negative = (input & FlagNullableNegativeNum) == FlagNullableNegativeNum;

			if (embedded)
			{
				if (negative)
				{
					long theNumber = input & FlagNullableNegativeEmbdeddedMask;

					return -theNumber;
				}
				else
				{
					return input & FlagEmbdeddedMask;
				}
			}
			else
			{
				var buff = SharedArray.Get();
				if (negative)
				{
					int length = input & FlagNullableNegativeMask;

					if (length < 8)
						SharedArray.ClearArray8();

					reader.Read(buff, 0, length);

					return -ConvertFromVarBinaryInt64(buff);
				}
				else
				{
					int length = input;
					if (length < 8)
						SharedArray.ClearArray8();

					reader.Read(buff, 0, length);

					return ConvertFromVarBinaryInt64(buff);
				}
			}
		}

		internal static long ReadVarInt64(BinaryReader reader)
		{
			var input = reader.ReadByte();

			var embedded = (input & FlagEmbdedded) == FlagEmbdedded;
			var negative = (input & FlagNonullNegativeNum) == FlagNonullNegativeNum;

			if (embedded)
			{
				if (negative)
				{
					var thenumber = input & FlagNonullNegativeNumEmbdeddedMask;

					return -thenumber;
				}
				return input & FlagEmbdeddedMask;
			}
			else
			{
				var buff = SharedArray.Get();
				if (negative)
				{
					int length = input & FlagNonullNegativeMask;

					if (length < 8)
						SharedArray.ClearArray8();

					reader.Read(buff, 0, length);

					return -ConvertFromVarBinaryInt64(buff);
				}
				else
				{
					int length = input;

					if (length < 8)
						SharedArray.ClearArray8();

					reader.Read(buff, 0, length);

					return ConvertFromVarBinaryInt64(buff);
				}
			}
		}

		internal static ulong? ReadVarUInt64Nullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == FlagNullable)
				return null;

			var embedded = (input & FlagEmbdedded) == FlagEmbdedded;

			if (embedded)
			{
				return (uint)input & FlagEmbdeddedMask;
			}
			else
			{
				int length = input;

				var buff = SharedArray.Get();
				if (length < 8)
					SharedArray.ClearArray8();

				reader.Read(buff, 0, length);

				return ConvertFromVarBinaryUInt64(buff);
			}
		}

		internal static ulong ReadVarUInt64(BinaryReader reader)
		{
			var input = reader.ReadByte();

			var embedded = (input & FlagEmbdedded) == FlagEmbdedded;

			if (embedded)
			{
				return (ulong)input & FlagEmbdeddedMask;
			}
			else
			{
				int length = input;

				var buff = SharedArray.Get();
				if (length < 8)
					SharedArray.ClearArray8();

				reader.Read(buff, 0, length);

				return ConvertFromVarBinaryUInt64(buff);
			}
		}

		internal static decimal? ReadVarDecimalNullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == FlagNullable)
				return null;

			var embedded = (input & FlagEmbdedded) == FlagEmbdedded;
			if (embedded)
			{
				return (input & FlagEmbdeddedMask);
			}

			int length = input;

			var numBuff = reader.ReadBytes(length);

			return ConvertFromVarBinaryDecimal(numBuff);
		}

		internal static decimal ReadVarDecimal(BinaryReader reader)
		{
			var input = reader.ReadByte();

			var embedded = (input & FlagEmbdedded) == FlagEmbdedded;
			if (embedded)
			{
				return (input & FlagEmbdeddedMask);
			}

			int length = input;

			var numBuff = reader.ReadBytes(length);

			return ConvertFromVarBinaryDecimal(numBuff);
		}

		internal static double? ReadVarDoubleNullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == FlagNullable)
				return null;

			var buff = SharedArray.Get();

			var embedded = (input & FlagEmbdedded) == FlagEmbdedded;
			if (embedded)
			{
				SharedArray.ClearArray8();

				// last byte
				buff[7] = (byte)(input & FlagEmbdeddedMask);
				return BitConverter.ToDouble(buff, 0);
			}

			int length = input;

			if (length < 8)
				SharedArray.ClearArray8();

			reader.Read(buff, 8 - length, length);

			return BitConverter.ToDouble(buff, 0);
		}

		internal static double ReadVarDouble(BinaryReader reader)
		{
			var input = reader.ReadByte();

			var buff = SharedArray.Get();

			var embedded = (input & FlagEmbdedded) == FlagEmbdedded;
			if (embedded)
			{
				SharedArray.ClearArray8();

				// last byte
				buff[7] = (byte)(input & FlagEmbdeddedMask);
				return BitConverter.ToDouble(buff, 0);
			}

			int length = input;

			if (length < 8)
				SharedArray.ClearArray8();

			reader.Read(buff, 8 - length, length);

			return BitConverter.ToDouble(buff, 0);
		}

		internal static float? ReadVarSingleNullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == FlagNullable)
				return null;

			var buff = SharedArray.Get();

			var embedded = (input & FlagEmbdedded) == FlagEmbdedded;
			if (embedded)
			{
				SharedArray.ClearArray4();

				// last byte
				buff[3] = (byte)(input & FlagEmbdeddedMask);
				return BitConverter.ToSingle(buff, 0);
			}

			int length = input;

			if (length < 4)
				SharedArray.ClearArray4();

			reader.Read(buff, 4 - length, length);

			return BitConverter.ToSingle(buff, 0);
		}

		internal static float ReadVarSingle(BinaryReader reader)
		{
			var input = reader.ReadByte();

			var buff = SharedArray.Get();

			var embedded = (input & FlagEmbdedded) == FlagEmbdedded;
			if (embedded)
			{
				SharedArray.ClearArray4();

				// last byte
				buff[3] = (byte)(input & FlagEmbdeddedMask);
				return BitConverter.ToSingle(buff, 0);
			}

			int length = input;

			if (length < 4)
				SharedArray.ClearArray4();

			reader.Read(buff, 4 - length, length);

			return BitConverter.ToSingle(buff, 0);
		}

		internal static byte? ReadVarByteNullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == FlagNullable)
				return null;

			var embedded = (input & FlagEmbdedded) == FlagEmbdedded;
			if (embedded)
			{
				return (byte)(input & FlagEmbdeddedMask);
			}

			return reader.ReadByte();
		}

		internal static sbyte? ReadVarSByteNullable(BinaryReader reader)
		{
			var input = reader.ReadSByte();
			if (input == FlagNullable)
				return null;

			var embedded = (input & FlagEmbdedded) == FlagEmbdedded;
			var negative = (input & FlagNullableNegativeNum) == FlagNullableNegativeNum;
			if (embedded)
			{
				if (negative)
				{
					var theNumber = -((sbyte)(input & FlagNullableNegativeEmbdeddedMask));

					return (sbyte)theNumber;
				}
				return (sbyte)(input & FlagEmbdeddedMask);
			}
			else
			{
				if (negative)
				{
					var number = -reader.ReadSByte();

					return (sbyte)number;
				}

				return reader.ReadSByte();
			}
		}

		#endregion

		#region Writers

		/// <summary>
		/// [EmbedIndicator-SignIndicator-0-0-0-0-0-0] [optional data]  0..63 can be embedded
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="num"></param>
		internal static void WriteVarInt(BinaryWriter writer, int num)
		{
			void WriteAsBigPositive()
			{
				var numBuff = NumericSerializers.ConvertToVarBinary(num, out var numLen);

				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}

			if (num > EmbeddedSignedMaxNumInByte)
			{
				// number is not negative
				// number is not embedded

				WriteAsBigPositive();
			}
			else if (num < 0)
			{
				if (num == int.MinValue)
				{
					// Very special case, int.minValut cannot be converted

					WriteAsBigPositive();
					return;
				}
				num = -num;
				if (num > EmbeddedSignedMaxNumInByte)
				{
					// number is negative
					// number is not embedded

					var numBuff = NumericSerializers.ConvertToVarBinary(num, out var numLen);

					writer.Write((byte)(numLen | FlagNonullNegativeNum));
					writer.Write(numBuff, 0, numLen);
				}
				else
				{
					// number is negative
					// number is embedded

					writer.Write((byte)(num | FlagNonullNegativeNumEmbdedded));
				}
			}
			else
			{
				// number is not negative
				// number is embedded

				writer.Write((byte)(num | FlagEmbdedded));
			}
		}

		/// <summary>
		/// [EmbedIndicator-NullIndicator-SignIndicator-0-0-0-0-0] [optional data]  0..31 can be embedded
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="num"></param>
		internal static void WriteVarInt(BinaryWriter writer, int? num)
		{
			void WriteAsBigPositive()
			{
				var numBuff = NumericSerializers.ConvertToVarBinary(num.Value, out var numLen);

				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}

			if (num == null)
			{
				// number is null

				writer.Write(FlagNullable);
			}
			else if (num > EmbeddedSignedNullableMaxNumInByte)
			{
				// number is not null
				// number is not negative
				// number is not embedded

				WriteAsBigPositive();
			}
			else if (num < 0)
			{
				if (num == int.MinValue)
				{
					// Very special case, int.minValut cannot be converted

					WriteAsBigPositive();
					return;
				}

				num = -num;
				if (num > EmbeddedSignedNullableMaxNumInByte)
				{
					// number is not null
					// number is negative
					// number is not embedded

					var numBuff = NumericSerializers.ConvertToVarBinary(num.Value, out var numLen);

					writer.Write((byte)(numLen | FlagNullableNegativeNum));
					writer.Write(numBuff, 0, numLen);
				}
				else
				{
					// number is not null
					// number is negative
					// number is embedded

					writer.Write((byte)(num.Value | FlagNullableNegativeEmbdeddedNum));
				}
			}
			else
			{
				// number is not null
				// number is not negative
				// number is embedded

				writer.Write((byte)(num.Value | FlagEmbdedded));
			}
		}

		/// <summary>
		/// [EmbedIndicator-0-0-0-0-0-0-0] [optional data]  0..127 can be embedded
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="num"></param>
		internal static void WriteVarInt(BinaryWriter writer, uint num)
		{
			if (num > EmbeddedUnsignedMaxNumInByte)
			{
				// number is not embedded

				var numBuff = NumericSerializers.ConvertToVarBinary(num, out var numLen);

				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}
			else
			{
				// number is embedded

				writer.Write((byte)(num | FlagEmbdedded));
			}
		}

		/// <summary>
		/// [NullIndicator-EmbedIndicator-0-0-0-0-0-0] [optional data]  0..127 can be embedded
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="num"></param>
		internal static void WriteVarInt(BinaryWriter writer, uint? num)
		{
			if (num == null)
			{
				// number is null

				writer.Write(FlagNullable);
			}
			else if (num > EmbeddedUnsignedNullableMaxNumInByte)
			{
				// number is not null
				// number is not negative
				// number is not embedded

				var numBuff = NumericSerializers.ConvertToVarBinary(num.Value, out var numLen);

				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}
			else
			{
				// number is not null
				// number is not negative
				// number is embedded

				writer.Write((byte)(num.Value | FlagEmbdedded));
			}
		}

		/// <summary>
		/// Same as "uint?" except that it doesn't store null value, but still preserves null flag
		/// </summary>
		/// <remarks>
		/// The value stored as member count can be 'null', but since this method is called where it is obvious that 
		/// the don't have null value, there is no point creating Nullable object to convert it
		/// </remarks>
		internal static void WriteUIntNullableMemberCount(BinaryWriter writer, uint num)
		{
			// NOTE:
			// Member count can be null, but not the place this method is being called
			// Hence why i'm storing null flag

			if (num > EmbeddedUnsignedNullableMaxNumInByte)
			{
				// number is not null
				// number is not negative
				// number is not embedded

				var numBuff = NumericSerializers.ConvertToVarBinary(num, out var numLen);

				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}
			else
			{
				// number is not null
				// number is not negative
				// number is embedded

				writer.Write((byte)(num | FlagEmbdedded));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarInt(BinaryWriter writer, short num)
		{
			void WriteAsBigPositive()
			{
				var numBuff = NumericSerializers.ConvertToVarBinary(num, out var numLen);

				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}

			if (num > EmbeddedSignedMaxNumInByte)
			{
				// number is not negative
				// number is not embedded
				WriteAsBigPositive();
			}
			else if (num < 0)
			{
				if (num == short.MinValue)
				{
					WriteAsBigPositive();
					return;
				}

				num = (short)-num;
				if (num > EmbeddedSignedMaxNumInByte)
				{
					// number is negative
					// number is not embedded

					var numBuff = NumericSerializers.ConvertToVarBinary(num, out var numLen);

					writer.Write((byte)(numLen | FlagNonullNegativeNum));
					writer.Write(numBuff, 0, numLen);
				}
				else
				{
					// number is negative
					// number is embedded

					writer.Write((byte)(num | FlagNonullNegativeNumEmbdedded));
				}
			}
			else
			{
				// number is not negative
				// number is embedded

				writer.Write((byte)(num | FlagEmbdedded));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarInt(BinaryWriter writer, short? num)
		{
			void WriteAsBigPositive()
			{
				var numBuff = NumericSerializers.ConvertToVarBinary(num.Value, out var numLen);

				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}

			if (num == null)
			{
				// number is null

				writer.Write(FlagNullable);
			}
			else if (num > EmbeddedSignedNullableMaxNumInByte)
			{
				// number is not null
				// number is not negative
				// number is not embedded

				WriteAsBigPositive();
			}
			else if (num < 0)
			{
				if (num == short.MinValue)
				{
					WriteAsBigPositive();
					return;
				}

				num = (short)-num;
				if (num > EmbeddedSignedNullableMaxNumInByte)
				{
					// number is not null
					// number is negative
					// number is not embedded

					var numBuff = NumericSerializers.ConvertToVarBinary(num.Value, out var numLen);

					writer.Write((byte)(numLen | FlagNullableNegativeNum));
					writer.Write(numBuff, 0, numLen);
				}
				else
				{
					// number is not null
					// number is negative
					// number is embedded

					writer.Write((byte)(num.Value | FlagNullableNegativeEmbdeddedNum));
				}
			}
			else
			{
				// number is not null
				// number is not negative
				// number is embedded

				writer.Write((byte)(num.Value | FlagEmbdedded));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarInt(BinaryWriter writer, ushort num)
		{
			if (num > EmbeddedUnsignedMaxNumInByte)
			{
				// number is not embedded

				var numBuff = NumericSerializers.ConvertToVarBinary(num, out var numLen);

				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}
			else
			{
				// number is embedded

				writer.Write((byte)(num | FlagEmbdedded));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarInt(BinaryWriter writer, ushort? num)
		{
			if (num == null)
			{
				// number is null

				writer.Write(FlagNullable);
			}
			else if (num > EmbeddedUnsignedNullableMaxNumInByte)
			{
				// number is not null
				// number is not negative
				// number is not embedded

				var numBuff = NumericSerializers.ConvertToVarBinary(num.Value, out var numLen);

				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}
			else
			{
				// number is not null
				// number is not negative
				// number is embedded

				writer.Write((byte)(num.Value | FlagEmbdedded));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarInt(BinaryWriter writer, long num)
		{
			void WriteAsBigPositive()
			{
				var numBuff = NumericSerializers.ConvertToVarBinary(num, out var numLen);

				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}

			if (num > EmbeddedSignedMaxNumInByte)
			{
				// number is not negative
				// number is not embedded

				WriteAsBigPositive();
			}
			else if (num < 0)
			{
				if (num == long.MinValue)
				{
					WriteAsBigPositive();
					return;
				}

				num = -num;
				if (num > EmbeddedSignedMaxNumInByte)
				{
					// number is negative
					// number is not embedded

					var numBuff = NumericSerializers.ConvertToVarBinary(num, out var numLen);

					writer.Write((byte)(numLen | FlagNonullNegativeNum));
					writer.Write(numBuff, 0, numLen);
				}
				else
				{
					// number is negative
					// number is embedded

					writer.Write((byte)(num | FlagNonullNegativeNumEmbdedded));
				}
			}
			else
			{
				// number is not negative
				// number is embedded

				writer.Write((byte)(num | FlagEmbdedded));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarInt(BinaryWriter writer, ulong num)
		{
			if (num > EmbeddedUnsignedMaxNumInByte)
			{
				// number is not embedded

				var numBuff = NumericSerializers.ConvertToVarBinary(num, out var numLen);

				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}
			else
			{
				// number is embedded

				writer.Write((byte)(num | FlagEmbdedded));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarInt(BinaryWriter writer, long? num)
		{
			void WriteAsBigPositive()
			{
				var numBuff = NumericSerializers.ConvertToVarBinary(num.Value, out var numLen);

				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}

			if (num == null)
			{
				// number is null

				writer.Write(FlagNullable);
			}
			else if (num > EmbeddedSignedNullableMaxNumInByte)
			{
				// number is not null
				// number is not negative
				// number is not embedded

				WriteAsBigPositive();
			}
			else if (num < 0)
			{
				if (num == long.MinValue)
				{
					WriteAsBigPositive();
					return;
				}

				num = -num;
				if (num > EmbeddedSignedNullableMaxNumInByte)
				{
					// number is not null
					// number is negative
					// number is not embedded

					var numBuff = NumericSerializers.ConvertToVarBinary(num.Value, out var numLen);

					writer.Write((byte)(numLen | FlagNullableNegativeNum));
					writer.Write(numBuff, 0, numLen);
				}
				else
				{
					// number is not null
					// number is negative
					// number is embedded

					writer.Write((byte)(num.Value | FlagNullableNegativeEmbdeddedNum));
				}
			}
			else
			{
				// number is not null
				// number is not negative
				// number is embedded

				writer.Write((byte)(num.Value | FlagEmbdedded));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarInt(BinaryWriter writer, ulong? num)
		{
			if (num == null)
			{
				// number is null

				writer.Write(FlagNullable);
			}
			else if (num > EmbeddedUnsignedNullableMaxNumInByte)
			{
				// number is not null
				// number is not negative
				// number is not embedded

				var numBuff = NumericSerializers.ConvertToVarBinary(num.Value, out var numLen);

				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}
			else
			{
				// number is not null
				// number is not negative
				// number is embedded

				writer.Write((byte)(num.Value | FlagEmbdedded));
			}
		}


		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarInt(BinaryWriter writer, byte? num)
		{
			if (num == null)
			{
				// number is null

				writer.Write(FlagNullable);
			}
			else if (num > EmbeddedUnsignedNullableMaxNumInByte)
			{
				// number is not null
				// number is not negative
				// number is not embedded

				writer.Write((byte)1);
				writer.Write(num.Value);
			}
			else
			{
				// number is not null
				// number is not negative
				// number is embedded

				writer.Write((byte)(num.Value | FlagEmbdedded));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarInt(BinaryWriter writer, sbyte? num)
		{
			void WriteAsBigPositive()
			{
				writer.Write((byte)1);
				writer.Write(num.Value);
			}

			if (num == null)
			{
				// number is null

				writer.Write(FlagNullable);
			}
			else if (num > EmbeddedSignedNullableMaxNumInByte)
			{
				// number is not null
				// number is not negative
				// number is not embedded

				WriteAsBigPositive();
			}
			else if (num < 0)
			{
				if (num == sbyte.MinValue)
				{
					WriteAsBigPositive();
					return;
				}

				num = (sbyte)-num;
				if (num > EmbeddedSignedNullableMaxNumInByte)
				{
					// number is not null
					// number is negative
					// number is not embedded

					byte numLen = 1;

					writer.Write((byte)(numLen | FlagNullableNegativeNum));
					writer.Write(num.Value);
				}
				else
				{
					// number is not null
					// number is negative
					// number is embedded

					writer.Write((sbyte)(num.Value | FlagNullableNegativeEmbdeddedNum));
				}
			}
			else
			{
				// number is not null
				// number is not negative
				// number is embedded

				writer.Write((sbyte)(num.Value | FlagEmbdedded));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarDecimal(BinaryWriter writer, float num)
		{
			var numBuff = NumericSerializers.ConvertToVarBinary(num, out var numLen, out var position);
			var firstByte = numBuff[position];

			if (numLen == 1 && firstByte <= EmbeddedUnsignedMaxNumInByte)
			{
				// number is embedded

				writer.Write((byte)(firstByte | FlagEmbdedded));
			}
			else
			{
				// number is not embedded
				writer.Write(numLen);
				writer.Write(numBuff, position, numLen);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarDecimal(BinaryWriter writer, float? num)
		{
			if (num == null)
			{
				// number is null

				writer.Write(FlagNullable);
				return;
			}

			var numBuff = NumericSerializers.ConvertToVarBinary(num.Value, out var numLen, out var position);
			var firstByte = numBuff[position];

			if (numLen == 1 && firstByte <= EmbeddedUnsignedNullableMaxNumInByte)
			{
				// number is embedded

				writer.Write((byte)(firstByte | FlagEmbdedded));
			}
			else
			{
				// number is not embedded
				writer.Write(numLen);
				writer.Write(numBuff, position, numLen);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarDecimal(BinaryWriter writer, double num)
		{
			var numBuff = NumericSerializers.ConvertToVarBinary(num, out var numLen, out var position);
			var firstByte = numBuff[position];

			if (numLen == 1 && firstByte <= EmbeddedUnsignedMaxNumInByte)
			{
				// number is embedded

				writer.Write((byte)(firstByte | FlagEmbdedded));
			}
			else
			{
				// number is not embedded
				writer.Write(numLen);
				writer.Write(numBuff, position, numLen);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarDecimal(BinaryWriter writer, double? num)
		{
			if (num == null)
			{
				// number is null

				writer.Write(FlagNullable);
				return;
			}

			var numBuff = NumericSerializers.ConvertToVarBinary(num.Value, out var numLen, out var position);
			var firstByte = numBuff[position];

			if (numLen == 1 && firstByte <= EmbeddedUnsignedNullableMaxNumInByte)
			{
				// number is embedded

				writer.Write((byte)(firstByte | FlagEmbdedded));
			}
			else
			{
				// number is not embedded
				writer.Write(numLen);
				writer.Write(numBuff, position, numLen);
			}
		}


		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarDecimal(BinaryWriter writer, decimal num)
		{
			var numBuff = NumericSerializers.ConvertToVarBinary(num, out var numLen);
			var firstByte = numBuff[0];

			if (numLen == 1 && firstByte <= EmbeddedUnsignedMaxNumInByte)
			{
				// number is embedded

				writer.Write((byte)(firstByte | FlagEmbdedded));
			}
			else
			{
				// number is not embedded
				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarDecimal(BinaryWriter writer, decimal? num)
		{
			if (num == null)
			{
				// number is null

				writer.Write(FlagNullable);
				return;
			}

			var numBuff = NumericSerializers.ConvertToVarBinary(num.Value, out var numLen);
			var firstByte = numBuff[0];

			if (numLen == 1 && firstByte <= EmbeddedUnsignedNullableMaxNumInByte)
			{
				// number is embedded

				writer.Write((byte)(firstByte | FlagEmbdedded));
			}
			else
			{
				// number is not embedded
				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}
		}
		#endregion

		#region Binary Converters

		private static short ConvertFromVarBinaryInt16(byte[] numBuff)
		{
			short result = numBuff[0];

			if (numBuff.Length == 1)
				return result;

			result = unchecked((short)((ushort)result | (numBuff[1] << 8)));
			return result;
		}

		private static ushort ConvertFromVarBinaryUInt16(byte[] numBuff)
		{
			ushort result;
			if (numBuff.Length == 4)
			{
				result = numBuff[0];
				result = unchecked((ushort)(short)(result | (numBuff[1] << 8)));
			}
			else
			{
				var len = numBuff.Length;

				result = numBuff[0];
				if (len == 1)
					return result;

				result = unchecked((ushort)(short)(result | (numBuff[1] << 8)));
				if (len == 2)
					return result;

				result = unchecked((ushort)(short)(result | (numBuff[2] << 16)));
				if (len == 3)
					return result;

				result = unchecked((ushort)(short)(result | (numBuff[3] << 24)));
			}
			return result;
		}

		private static int ConvertFromVarBinaryInt32(byte[] numBuff, int len)
		{
			int result;
			if (len == 4)
			{
				result = numBuff[0];
				result = unchecked(result | (numBuff[1] << 8));
				result = unchecked(result | (numBuff[2] << 16));
				result = unchecked(result | (numBuff[3] << 24));
			}
			else
			{
				result = numBuff[0];
				if (len == 1)
					return result;

				result = unchecked(result | (numBuff[1] << 8));
				if (len == 2)
					return result;

				result = unchecked(result | (numBuff[2] << 16));
				if (len == 3)
					return result;

				result = unchecked(result | (numBuff[3] << 24));
			}
			return result;
		}

		private static int ConvertFromVarBinaryInt32StartIndex(byte[] numBuff, int startIndex)
		{
			return ((numBuff[startIndex + 0] | (numBuff[startIndex + 1] << 8)) | (numBuff[startIndex + 2] << 16)) | (numBuff[startIndex + 3] << 24);
		}

		private static uint ConvertFromVarBinaryUInt32(byte[] numBuff, int len)
		{
			uint result;
			if (len == 4)
			{
				result = numBuff[0];
				result = unchecked((uint)((int)result | (numBuff[1] << 8)));
				result = unchecked((uint)((int)result | (numBuff[2] << 16)));
				result = unchecked((uint)((int)result | (numBuff[3] << 24)));
			}
			else
			{
				result = numBuff[0];
				if (len == 1)
					return result;

				result = unchecked((uint)((int)result | (numBuff[1] << 8)));
				if (len == 2)
					return result;

				result = unchecked((uint)((int)result | (numBuff[2] << 16)));
				if (len == 3)
					return result;

				result = unchecked((uint)((int)result | (numBuff[3] << 24)));
			}
			return result;
		}

		private static long ConvertFromVarBinaryInt64(byte[] numBuff)
		{
			uint num = unchecked((uint)(((numBuff[0] | (numBuff[1] << 8)) | (numBuff[2] << 16)) | (numBuff[3] << 24)));
			uint num2 = unchecked((uint)(((numBuff[4] | (numBuff[5] << 8)) | (numBuff[6] << 16)) | (numBuff[7] << 24)));
			return unchecked((long)((((ulong)num2) << 32) | ((ulong)num)));
		}

		private static ulong ConvertFromVarBinaryUInt64(byte[] numBuff)
		{
			uint num = unchecked((uint)(((numBuff[0] | (numBuff[1] << 8)) | (numBuff[2] << 16)) | (numBuff[3] << 24)));
			uint num2 = unchecked((uint)(((numBuff[4] | (numBuff[5] << 8)) | (numBuff[6] << 16)) | (numBuff[7] << 24)));
			return unchecked(((ulong)num2 << 32) | (ulong)num);

		}

		private static decimal ConvertFromVarBinaryDecimal(byte[] numBuff)
		{
			byte[] buff;
			if (numBuff.Length < 16)
			{
				// TODO: check why SharedArray.Get() decreases performance by 3x times
				buff = new byte[16]; // 16 required
				Array.Copy(numBuff, 0, buff, 0, numBuff.Length);
			}
			else
			{
				buff = numBuff;
			}

			var decimalBits = new int[4];
			decimalBits[0] = ConvertFromVarBinaryInt32StartIndex(buff, 4 * 0);
			decimalBits[1] = ConvertFromVarBinaryInt32StartIndex(buff, 4 * 1);
			decimalBits[2] = ConvertFromVarBinaryInt32StartIndex(buff, 4 * 2);
			decimalBits[3] = ConvertFromVarBinaryInt32StartIndex(buff, 4 * 3);

			return new decimal(decimalBits);
		}

		private static double ConvertFromVarBinaryDouble(byte[] numBuff)
		{
			if (numBuff.Length == 8)
			{
				return BitConverter.ToDouble(numBuff, 0);
			}
			else
			{
				var doubleBuff = new byte[8];

				Array.Copy(numBuff, 0, doubleBuff, 8 - numBuff.Length, numBuff.Length);
				return BitConverter.ToDouble(doubleBuff, 0);
			}
		}

		private static float ConvertFromVarBinarySingle(byte[] numBuff)
		{
			if (numBuff.Length == 4)
			{
				return BitConverter.ToSingle(numBuff, 0);
			}
			else
			{
				var doubleBuff = new byte[4];

				Array.Copy(numBuff, 0, doubleBuff, 4 - numBuff.Length, numBuff.Length);
				return BitConverter.ToSingle(doubleBuff, 0);
			}
		}

		private static byte[] ConvertToVarBinary(int value, out byte length)
		{
			if (value < 0)
			{
				length = 4;
				var buff = SharedArray.Get();
				buff[0] = (byte)value;
				buff[1] = unchecked((byte)(value >> 8));
				buff[2] = unchecked((byte)(value >> 16));
				buff[3] = unchecked((byte)(value >> 24));
				return buff;
			}
			else
			{
				var buff = SharedArray.Get();
				SharedArray.ClearArray4();

				var num1 = (byte)value;
				var num2 = unchecked((byte)(value >> 8));
				var num3 = unchecked((byte)(value >> 16));
				var num4 = unchecked((byte)(value >> 24));


				buff[0] = num1;

				if (num2 > 0)
				{
					buff[1] = num2;
				}
				else if (num3 == 0 && num4 == 0)
				{
					length = 1;
					return buff;
				}

				if (num3 > 0)
				{
					buff[2] = num3;
				}
				else if (num4 == 0)
				{
					length = 2;
					return buff;
				}

				if (num4 > 0)
					buff[3] = num4;
				else
				{
					length = 3;
					return buff;
				}
				length = 4;
				return buff;
			}
		}

		private static byte[] ConvertToVarBinary(uint value, out byte length)
		{
			if (value == 0)
			{
				length = 1;
				return new byte[] { 0 };
			}

			var buff = SharedArray.Get();
			SharedArray.ClearArray4();

			var num1 = (byte)value;
			var num2 = unchecked((byte)(value >> 8));
			var num3 = unchecked((byte)(value >> 16));
			var num4 = unchecked((byte)(value >> 24));


			buff[0] = num1;

			if (num2 > 0)
			{
				buff[1] = num2;
			}
			else if (num3 == 0 && num4 == 0)
			{
				length = 1;
				return buff;
			}

			if (num3 > 0)
			{
				buff[2] = num3;
			}
			else if (num4 == 0)
			{
				length = 2;
				return buff;
			}

			if (num4 > 0)
				buff[3] = num4;
			else
			{
				length = 3;
				return buff;
			}
			length = 4;
			return buff;
		}

		private static byte[] ConvertToVarBinary(long value, out byte length)
		{
			var buff = SharedArray.Get();
			buff[0] = (byte)value;
			buff[1] = unchecked((byte)(value >> 8));
			buff[2] = unchecked((byte)(value >> 16));
			buff[3] = unchecked((byte)(value >> 24));
			buff[4] = unchecked((byte)(value >> 32));
			buff[5] = unchecked((byte)(value >> 40));
			buff[6] = unchecked((byte)(value >> 48));
			buff[7] = unchecked((byte)(value >> 56));

			for (int i = 8 - 1; i >= 0; i--)
			{
				if (buff[i] > 0)
				{
					length = (byte)(i + 1);
					//if (length != 8)
					//	Array.Resize(ref buff, length);
					return buff;
				}
			}

			length = 1;
			return new byte[] { 0 };
		}

		private static byte[] ConvertToVarBinary(ulong value, out byte length)
		{
			var buff = SharedArray.Get();
			buff[0] = (byte)value;
			buff[1] = unchecked((byte)(value >> 8));
			buff[2] = unchecked((byte)(value >> 16));
			buff[3] = unchecked((byte)(value >> 24));
			buff[4] = unchecked((byte)(value >> 32));
			buff[5] = unchecked((byte)(value >> 40));
			buff[6] = unchecked((byte)(value >> 48));
			buff[7] = unchecked((byte)(value >> 56));

			for (int i = 8 - 1; i >= 0; i--)
			{
				if (buff[i] > 0)
				{
					length = (byte)(i + 1);
					return buff;
				}
			}

			length = 1;
			return new byte[] { 0 };
		}

		private static byte[] ConvertToVarBinary(short value, out byte length)
		{
			if (value < 0)
			{
				length = 2;
				var buff = new byte[2];
				buff[0] = (byte)value;
				buff[1] = unchecked((byte)(value >> 8));
				return buff;
			}
			else
			{
				var buff = new byte[2];
				var num1 = (byte)value;
				var num2 = unchecked((byte)(value >> 8));

				buff[0] = num1;

				if (num2 > 0)
					buff[1] = num2;
				else
				{
					length = 1;
					return buff;
				}

				length = 2;
				return buff;
			}
		}

		private static byte[] ConvertToVarBinary(ushort value, out byte length)
		{
			var buff = new byte[2];
			var num1 = (byte)value;
			var num2 = unchecked((byte)(value >> 8));

			buff[0] = num1;

			if (num2 > 0)
				buff[1] = num2;
			else
			{
				length = 1;
				return buff;
			}

			length = 2;
			return buff;
		}

		private static byte[] ConvertToVarBinary(float value, out byte length, out int position)
		{
			var bitsArray = BitConverter.GetBytes(value);
			for (int i = 0; i < 4; i++)
			{
				if (bitsArray[i] > 0)
				{
					position = i;
					length = (byte)(4 - position);

					return bitsArray;
				}
			}
			length = 1;
			position = 0;
			return new byte[] { 0 };
		}

		private static byte[] ConvertToVarBinary(float value, out byte length)
		{
			// Float & double numeric formats stores valuable bytes from right to left

			var valueBuff = BitConverter.GetBytes(value);
			var num1 = valueBuff[0];
			var num2 = valueBuff[1];
			var num3 = valueBuff[2];
			var num4 = valueBuff[3];

			// zero
			if (num4 == 0 && num3 == 0 && num2 == 0 && num1 == 0)
			{
				length = 1;
				return new byte[] { 0 };
			}

			if (num3 == 0 && num2 == 0 && num1 == 0)
			{
				length = 1;
				return new byte[] { num4 };
			}

			if (num2 == 0 && num1 == 0)
			{
				length = 2;
				return new byte[] { num3, num4 };
			}

			if (num1 == 0)
			{
				length = 3;
				return new byte[] { num2, num3, num4 };
			}

			// no zeros
			length = 4;

			return valueBuff;
		}

		private static byte[] ConvertToVarBinary(double value, out byte length, out int position)
		{
			var bitsArray = BitConverter.GetBytes(value);
			for (int i = 0; i < 8; i++)
			{
				if (bitsArray[i] > 0)
				{
					position = i;
					length = (byte)(8 - position);

					return bitsArray;
				}
			}
			length = 1;
			position = 0;
			return new byte[] { 0 };
		}

		private static byte[] ConvertToVarBinary(double value, out byte length)
		{
			// Float  &double numeric formats stores valuable bytes from right to left

			var valueBuff = BitConverter.GetBytes(value);
			var num1 = valueBuff[0];
			var num2 = valueBuff[1];
			var num3 = valueBuff[2];
			var num4 = valueBuff[3];
			var num5 = valueBuff[4];
			var num6 = valueBuff[5];
			var num7 = valueBuff[6];
			var num8 = valueBuff[7];

			// zero
			if (num8 == 0 && num7 == 0 && num6 == 0 && num5 == 0 && num4 == 0 && num3 == 0 && num2 == 0 && num1 == 0)
			{
				length = 1;
				return new byte[] { 0 };
			}

			if (num7 == 0 && num6 == 0 && num5 == 0 && num4 == 0 && num3 == 0 && num2 == 0 && num1 == 0)
			{
				length = 1;
				return new byte[] { num8 };
			}

			if (num6 == 0 && num5 == 0 && num4 == 0 && num3 == 0 && num2 == 0 && num1 == 0)
			{
				length = 2;
				return new byte[] { num7, num8 };
			}

			if (num5 == 0 && num4 == 0 && num3 == 0 && num2 == 0 && num1 == 0)
			{
				length = 3;
				return new byte[] { num6, num7, num8 };
			}

			if (num4 == 0 && num3 == 0 && num2 == 0 && num1 == 0)
			{
				length = 4;
				return new byte[] { num5, num6, num7, num8 };
			}

			if (num3 == 0 && num2 == 0 && num1 == 0)
			{
				length = 5;
				return new byte[] { num4, num5, num6, num7, num8 };
			}

			if (num2 == 0 && num1 == 0)
			{
				length = 6;
				return new byte[] { num3, num4, num5, num6, num7, num8 };
			}

			if (num1 == 0)
			{
				length = 7;
				return new byte[] { num2, num3, num4, num5, num6, num7, num8 };
			}

			// no zeros
			length = 8;

			return valueBuff;
		}

		private static byte[] ConvertToVarBinary(decimal value, out byte length)
		{
			var bits = decimal.GetBits(value);
			var bitsArray = SharedArray.Get();

			for (byte i = 0; i < bits.Length; i++)
			{
				var bytes = BitConverter.GetBytes(bits[i]);
				Array.Copy(bytes, 0, bitsArray, i * 4, 4);
			}

			// finding the empty characters
			for (int i = bitsArray.Length - 1; i >= 0; i--)
			{
				if (bitsArray[i] > 0)
				{
					length = (byte)(i + 1);

					return bitsArray;
				}
			}
			length = 1;
			return new byte[] { 0 };
		}

		#endregion


	}
}
