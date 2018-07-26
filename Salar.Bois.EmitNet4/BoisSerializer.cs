﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using Salar.Bois.Serializers;
using Salar.Bois.Types;

/* 
 * Salar BOIS (Binary Object Indexed Serialization)
 * by Salar Khalilzadeh
 * 
 * https://github.com/salarcode/Bois
 * Mozilla Public License v2
 */
namespace Salar.Bois
{
	/// <summary>
	/// Salar.Bois serializer.
	/// Which provides binary serialization and deserialzation for .NET objects.
	/// BOIS stands for 'Binary Object Indexed Serialization'.
	/// </summary>
	/// <Author>
	/// Salar Khalilzadeh
	/// </Author>
	public class BoisSerializer
	{

		/// <summary>
		/// Character encoding for strings.
		/// </summary>
		public Encoding Encoding { get; set; }

		/// <summary>
		/// Initializing a new instance of Bois serializar.
		/// </summary>
		public BoisSerializer()
		{
			Encoding = Encoding.UTF8;
		}

		public void Initialize<T>()
		{
			BoisTypeCache.GetRootTypeComputed(typeof(T), true, true);
		}

		public void Initialize(params Type[] types)
		{
			if (types == null)
				return;
			foreach (var type in types)
			{
				if (type != null)
					BoisTypeCache.GetRootTypeComputed(type, true, true);
			}
		}

		/// <summary>
		/// Use with caution, all the calculations will be lost and will be done again when needed
		/// </summary>
		public static void ClearCache()
		{
			BoisTypeCache.ClearCache();
		}

		/// <summary>
		/// Serializing an object to binary bois format.
		/// </summary>
		/// <param name="obj">The object to be serialized.</param>
		/// <param name="output">The output of the serialization in binary.</param>
		/// <typeparam name="T">The object type.</typeparam>
		public void Serialize<T>(T obj, Stream output)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj), "Object cannot be null.");
			//_serializeDepth = 0;
			var writer = new BinaryWriter(output, Encoding);

			var type = typeof(T);
			var typeInfo = BoisTypeCache.GetBasicType(type);
			if (typeInfo.AsRootNeedsCompute)
			{
				var computedType = BoisTypeCache.GetRootTypeComputed(type, false, true);

				computedType.InvokeWriter(writer, obj, Encoding);
			}
			else
			{
				WriteRootBasicType(writer, obj, type, typeInfo);
			}
		}

		/// <summary>
		/// Deserilizing binary data to a new instance.
		/// </summary>
		/// <param name="objectData">The binary data.</param>
		/// <typeparam name="T">The object type.</typeparam>
		/// <returns>New instance of the deserialized data.</returns>
		public T Deserialize<T>(Stream objectData)
		{
			var reader = new BinaryReader(objectData, Encoding);

			var type = typeof(T);
			var typeInfo = BoisTypeCache.GetBasicType(type);

			if (typeInfo.AsRootNeedsCompute)
			{
				var computedType = BoisTypeCache.GetRootTypeComputed(type, true, false);

				return computedType.InvokeReader<T>(reader, Encoding);
			}
			else
			{
				return (T)ReadRootBasicType(reader, type, typeInfo);
			}
		}

		#region Serialization

		private void WriteRootBasicType(BinaryWriter writer, object obj, Type type, BoisBasicTypeInfo typeInfo)
		{
			switch (typeInfo.KnownType)
			{
				case EnBasicKnownType.String:
					PrimitiveWriter.WriteValue(writer, obj as string, Encoding);
					return;

				case EnBasicKnownType.Char:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(writer, obj as char?);
					else
						PrimitiveWriter.WriteValue(writer, (char)obj);
					return;

				case EnBasicKnownType.Guid:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(writer, obj as Guid?);
					else
						PrimitiveWriter.WriteValue(writer, (Guid)obj);
					return;

				case EnBasicKnownType.Bool:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(writer, obj as bool?);
					else
						PrimitiveWriter.WriteValue(writer, (bool)obj);
					return;

				case EnBasicKnownType.Enum:
					PrimitiveWriter.WriteValue(writer, obj as Enum);
					return;

				case EnBasicKnownType.DateTime:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(writer, obj as DateTime?);
					else
						PrimitiveWriter.WriteValue(writer, (DateTime)obj);
					return;

				case EnBasicKnownType.DateTimeOffset:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(writer, obj as DateTimeOffset?);
					else
						PrimitiveWriter.WriteValue(writer, (DateTimeOffset)obj);
					return;

				case EnBasicKnownType.TimeSpan:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(writer, obj as TimeSpan?);
					else
						PrimitiveWriter.WriteValue(writer, (TimeSpan)obj);
					return;

				case EnBasicKnownType.ByteArray:
					PrimitiveWriter.WriteValue(writer, obj as byte[]);
					return;

				case EnBasicKnownType.KnownTypeArray:

					// calling for subitem
					WriteRootBasicTypedArray(writer, obj as Array, typeInfo);
					return;

				case EnBasicKnownType.Color:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(writer, obj as Color?);
					else
						PrimitiveWriter.WriteValue(writer, (Color)obj);
					break;

				case EnBasicKnownType.Version:
					PrimitiveWriter.WriteValue(writer, obj as Version);
					return;

				case EnBasicKnownType.DbNull:
					PrimitiveWriter.WriteValue(writer, obj as DBNull);
					return;

				case EnBasicKnownType.Uri:
					PrimitiveWriter.WriteValue(writer, (obj as Uri)?.ToString(), Encoding);
					break;

				case EnBasicKnownType.Int16:
					if (typeInfo.IsNullable)
						PrimitivesConvertion.WriteVarInt(writer, obj as short?);
					else
						PrimitivesConvertion.WriteVarInt(writer, (short)obj);
					break;

				case EnBasicKnownType.Int32:
					if (typeInfo.IsNullable)
						PrimitivesConvertion.WriteVarInt(writer, obj as int?);
					else
						PrimitivesConvertion.WriteVarInt(writer, (int)obj);
					return;

				case EnBasicKnownType.Int64:
					if (typeInfo.IsNullable)
						PrimitivesConvertion.WriteVarInt(writer, obj as long?);
					else
						PrimitivesConvertion.WriteVarInt(writer, (long)obj);
					return;

				case EnBasicKnownType.UInt16:
					if (typeInfo.IsNullable)
						PrimitivesConvertion.WriteVarInt(writer, obj as ushort?);
					else
						PrimitivesConvertion.WriteVarInt(writer, (ushort)obj);
					return;

				case EnBasicKnownType.UInt32:
					if (typeInfo.IsNullable)
						PrimitivesConvertion.WriteVarInt(writer, obj as uint?);
					else
						PrimitivesConvertion.WriteVarInt(writer, (uint)obj);
					return;

				case EnBasicKnownType.UInt64:
					if (typeInfo.IsNullable)
						PrimitivesConvertion.WriteVarInt(writer, obj as ulong?);
					else
						PrimitivesConvertion.WriteVarInt(writer, (ulong)obj);
					return;

				case EnBasicKnownType.Double:
					if (typeInfo.IsNullable)
						PrimitivesConvertion.WriteVarDecimal(writer, obj as double?);
					else
						PrimitivesConvertion.WriteVarDecimal(writer, (double)obj);
					return;

				case EnBasicKnownType.Decimal:
					if (typeInfo.IsNullable)
						PrimitivesConvertion.WriteVarDecimal(writer, obj as decimal?);
					else
						PrimitivesConvertion.WriteVarDecimal(writer, (decimal)obj);
					return;

				case EnBasicKnownType.Single:
					if (typeInfo.IsNullable)
						PrimitivesConvertion.WriteVarDecimal(writer, obj as float?);
					else
						PrimitivesConvertion.WriteVarDecimal(writer, (float)obj);
					return;

				case EnBasicKnownType.Byte:
					if (typeInfo.IsNullable)
						PrimitivesConvertion.WriteVarInt(writer, obj as byte?);
					else
						writer.Write((byte)obj);
					return;

				case EnBasicKnownType.SByte:
					if (typeInfo.IsNullable)
						PrimitivesConvertion.WriteVarInt(writer, obj as sbyte?);
					else
						writer.Write((sbyte)obj);
					return;


				case EnBasicKnownType.Unknown:
				default:
					// should never reach here
					throw new Exception($"Not supported type '{type}' as root");
			}
		}

		private void WriteRootBasicTypedArray(BinaryWriter writer, Array array, BoisBasicTypeInfo typeInfo)
		{
			if (array == null)
			{
				PrimitiveWriter.WriteNullableType(writer, true);
				return;
			}

			var arrayItemType = typeInfo.BareType;
			var arrayItemTypeType = BoisTypeCache.GetBasicType(typeInfo.BareType);

			// Int32
			PrimitivesConvertion.WriteVarInt(writer, (uint?)array.Length);

			for (int i = 0; i < array.Length; i++)
			{
				WriteRootBasicType(writer, array.GetValue(i), arrayItemType, arrayItemTypeType);
			}

		}


		#endregion

		#region Deseialization

		private object ReadRootBasicType(BinaryReader reader, Type type, BoisBasicTypeInfo typeInfo)
		{
			switch (typeInfo.KnownType)
			{
				case EnBasicKnownType.String:
					return PrimitiveReader.ReadString(reader, Encoding);

				case EnBasicKnownType.Char:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadCharNullable(reader);
					return PrimitiveReader.ReadChar(reader);

				case EnBasicKnownType.Guid:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadGuidNullable(reader);
					return PrimitiveReader.ReadGuid(reader);

				case EnBasicKnownType.Bool:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadBooleanNullable(reader);
					return PrimitiveReader.ReadBoolean(reader);

				case EnBasicKnownType.Enum:
					return PrimitiveReader.ReadEnum(reader);

				case EnBasicKnownType.DateTime:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadDateTimeNullable(reader);
					return PrimitiveReader.ReadDateTime(reader);

				case EnBasicKnownType.DateTimeOffset:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadDateTimeOffsetNullable(reader);
					return PrimitiveReader.ReadDateTimeOffset(reader);

				case EnBasicKnownType.TimeSpan:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadTimeSpanNullable(reader);
					return PrimitiveReader.ReadTimeSpan(reader);

				case EnBasicKnownType.ByteArray:
					return PrimitiveReader.ReadByteArray(reader);

				case EnBasicKnownType.KnownTypeArray:
					return ReadRootBasicTypedArray(reader, typeInfo);

				case EnBasicKnownType.Color:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadColorNullable(reader);
					return PrimitiveReader.ReadColor(reader);

				case EnBasicKnownType.Version:
					return PrimitiveReader.ReadVersion(reader);

				case EnBasicKnownType.DbNull:
					return PrimitiveReader.ReadDBNull(reader);

				case EnBasicKnownType.Uri:
					return PrimitiveReader.ReadUri(reader);

				case EnBasicKnownType.Int16:
					if (typeInfo.IsNullable)
						return PrimitivesConvertion.ReadVarInt16Nullable(reader);
					else
						return PrimitivesConvertion.ReadVarInt16(reader);

				case EnBasicKnownType.Int32:
					if (typeInfo.IsNullable)
						return PrimitivesConvertion.ReadVarInt32Nullable(reader);
					else
						return PrimitivesConvertion.ReadVarInt32(reader);

				case EnBasicKnownType.Int64:
					if (typeInfo.IsNullable)
						return PrimitivesConvertion.ReadVarInt64Nullable(reader);
					else
						return PrimitivesConvertion.ReadVarInt64(reader);

				case EnBasicKnownType.UInt16:
					if (typeInfo.IsNullable)
						return PrimitivesConvertion.ReadVarUInt16Nullable(reader);
					else
						return PrimitivesConvertion.ReadVarUInt16(reader);

				case EnBasicKnownType.UInt32:
					if (typeInfo.IsNullable)
						return PrimitivesConvertion.ReadVarUInt32Nullable(reader);
					else
						return PrimitivesConvertion.ReadVarUInt32(reader);

				case EnBasicKnownType.UInt64:
					if (typeInfo.IsNullable)
						return PrimitivesConvertion.ReadVarUInt64Nullable(reader);
					else
						return PrimitivesConvertion.ReadVarUInt64(reader);

				case EnBasicKnownType.Double:
					if (typeInfo.IsNullable)
						return PrimitivesConvertion.ReadVarDoubleNullable(reader);
					else
						return PrimitivesConvertion.ReadVarDouble(reader);


				case EnBasicKnownType.Decimal:
					if (typeInfo.IsNullable)
						return PrimitivesConvertion.ReadVarDecimalNullable(reader);
					else
						return PrimitivesConvertion.ReadVarDecimal(reader);

				case EnBasicKnownType.Single:
					if (typeInfo.IsNullable)
						return PrimitivesConvertion.ReadVarSingleNullable(reader);
					else
						return PrimitivesConvertion.ReadVarSingle(reader);

				case EnBasicKnownType.Byte:
					if (typeInfo.IsNullable)
						return PrimitivesConvertion.ReadVarByteNullable(reader);
					else
						return reader.ReadByte();

				case EnBasicKnownType.SByte:
					if (typeInfo.IsNullable)
						return (sbyte?)PrimitivesConvertion.ReadVarByteNullable(reader);
					else
						return reader.ReadSByte();

				case EnBasicKnownType.Unknown:
					//
					break;
			}
			throw new Exception($"Not supported type '{type}' as root");
		}

		private Array ReadRootBasicTypedArray(BinaryReader reader, BoisBasicTypeInfo typeInfo)
		{
			var length = PrimitivesConvertion.ReadVarUInt32Nullable(reader);
			if (length == null)
			{
				return null;
			}

			var arrayItemType = typeInfo.BareType;
			var arrayItemTypeType = BoisTypeCache.GetBasicType(typeInfo.BareType);

			var result = Array.CreateInstance(arrayItemType, (int)length.Value);

			for (int i = 0; i < length; i++)
			{
				var item = ReadRootBasicType(reader, arrayItemType, arrayItemTypeType);
				result.SetValue(item, i);
			}
			return result;
		}
		#endregion
	}
}
