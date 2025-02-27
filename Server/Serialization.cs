#region References
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

using Server.Guilds;
#endregion

namespace Server
{
	public abstract class GenericReader
	{
		public abstract Type ReadObjectType();

		public abstract string ReadString();
		public abstract DateTime ReadDateTime();
		public abstract DateTimeOffset ReadDateTimeOffset();
		public abstract TimeSpan ReadTimeSpan();
		public abstract DateTime ReadDeltaTime();
		public abstract decimal ReadDecimal();
		public abstract long ReadLong();
		public abstract ulong ReadULong();
		public abstract int PeekInt();
		public abstract int ReadInt();
		public abstract uint ReadUInt();
		public abstract short ReadShort();
		public abstract ushort ReadUShort();
		public abstract double ReadDouble();
		public abstract float ReadFloat();
		public abstract char ReadChar();
		public abstract byte ReadByte();
		public abstract sbyte ReadSByte();
		public abstract bool ReadBool();
		public abstract int ReadEncodedInt();
		public abstract IPAddress ReadIPAddress();

		public abstract Point3D ReadPoint3D();
		public abstract Point2D ReadPoint2D();
		public abstract Rectangle2D ReadRect2D();
		public abstract Rectangle3D ReadRect3D();
		public abstract Map ReadMap();

		public abstract Serial ReadSerial();

		public abstract Item ReadItem();
		public abstract Mobile ReadMobile();
		public abstract BaseGuild ReadGuild();

		public abstract T ReadItem<T>() where T : Item;
		public abstract T ReadMobile<T>() where T : Mobile;
		public abstract T ReadGuild<T>() where T : BaseGuild;

		public abstract ArrayList ReadItemList();
		public abstract ArrayList ReadMobileList();
		public abstract ArrayList ReadGuildList();

		public abstract List<Item> ReadStrongItemList();
		public abstract List<T> ReadStrongItemList<T>() where T : Item;

		public abstract List<Mobile> ReadStrongMobileList();
		public abstract List<T> ReadStrongMobileList<T>() where T : Mobile;

		public abstract List<BaseGuild> ReadStrongGuildList();
		public abstract List<T> ReadStrongGuildList<T>() where T : BaseGuild;

		public abstract HashSet<Item> ReadItemSet();
		public abstract HashSet<T> ReadItemSet<T>() where T : Item;

		public abstract HashSet<Mobile> ReadMobileSet();
		public abstract HashSet<T> ReadMobileSet<T>() where T : Mobile;

		public abstract HashSet<BaseGuild> ReadGuildSet();
		public abstract HashSet<T> ReadGuildSet<T>() where T : BaseGuild;

		public abstract Race ReadRace();

		public abstract bool End();
	}

	public abstract class GenericWriter
	{
		public abstract void Close();

		public abstract long Position { get; }

		public abstract void WriteObjectType(object value);
		public abstract void WriteObjectType(Type value);

		public abstract void Write(string value);
		public abstract void Write(DateTime value);
		public abstract void Write(DateTimeOffset value);
		public abstract void Write(TimeSpan value);
		public abstract void Write(decimal value);
		public abstract void Write(long value);
		public abstract void Write(ulong value);
		public abstract void Write(int value);
		public abstract void Write(uint value);
		public abstract void Write(short value);
		public abstract void Write(ushort value);
		public abstract void Write(double value);
		public abstract void Write(float value);
		public abstract void Write(char value);
		public abstract void Write(byte value);
		public abstract void Write(sbyte value);
		public abstract void Write(bool value);
		public abstract void WriteEncodedInt(int value);
		public abstract void Write(IPAddress value);
		public abstract void Write(Serial value);

		public abstract void WriteDeltaTime(DateTime value);

		public abstract void Write(Point3D value);
		public abstract void Write(Point2D value);
		public abstract void Write(Rectangle2D value);
		public abstract void Write(Rectangle3D value);
		public abstract void Write(Map value);

		public abstract void Write(Item value);
		public abstract void Write(Mobile value);
		public abstract void Write(BaseGuild value);

		public abstract void Write(Race value);

		public abstract void WriteItemList(ArrayList list);
		public abstract void WriteItemList(ArrayList list, bool tidy);

		public abstract void WriteMobileList(ArrayList list);
		public abstract void WriteMobileList(ArrayList list, bool tidy);

		public abstract void WriteGuildList(ArrayList list);
		public abstract void WriteGuildList(ArrayList list, bool tidy);

		public abstract void Write(List<Item> list);
		public abstract void Write(List<Item> list, bool tidy);

		public abstract void WriteItemList<T>(List<T> list) where T : Item;
		public abstract void WriteItemList<T>(List<T> list, bool tidy) where T : Item;

		public abstract void Write(HashSet<Item> list);
		public abstract void Write(HashSet<Item> list, bool tidy);

		public abstract void WriteItemSet<T>(HashSet<T> set) where T : Item;
		public abstract void WriteItemSet<T>(HashSet<T> set, bool tidy) where T : Item;

		public abstract void Write(List<Mobile> list);
		public abstract void Write(List<Mobile> list, bool tidy);

		public abstract void WriteMobileList<T>(List<T> list) where T : Mobile;
		public abstract void WriteMobileList<T>(List<T> list, bool tidy) where T : Mobile;

		public abstract void Write(HashSet<Mobile> list);
		public abstract void Write(HashSet<Mobile> list, bool tidy);

		public abstract void WriteMobileSet<T>(HashSet<T> set) where T : Mobile;
		public abstract void WriteMobileSet<T>(HashSet<T> set, bool tidy) where T : Mobile;

		public abstract void Write(List<BaseGuild> list);
		public abstract void Write(List<BaseGuild> list, bool tidy);

		public abstract void WriteGuildList<T>(List<T> list) where T : BaseGuild;
		public abstract void WriteGuildList<T>(List<T> list, bool tidy) where T : BaseGuild;

		public abstract void Write(HashSet<BaseGuild> list);
		public abstract void Write(HashSet<BaseGuild> list, bool tidy);

		public abstract void WriteGuildSet<T>(HashSet<T> set) where T : BaseGuild;
		public abstract void WriteGuildSet<T>(HashSet<T> set, bool tidy) where T : BaseGuild;
	}

	public class BinaryFileWriter : GenericWriter
	{
		private readonly bool PrefixStrings;
		private readonly Stream m_File;

		protected virtual int BufferSize => 81920;

		private readonly byte[] m_Buffer;

		private int m_Index;

		private readonly Encoding m_Encoding;

		public BinaryFileWriter(Stream strm, bool prefixStr)
		{
			PrefixStrings = prefixStr;

			m_Encoding = Utility.UTF8;
			m_Buffer = new byte[BufferSize];
			m_File = strm;
		}

		public BinaryFileWriter(string filename, bool prefixStr)
			: this(filename, prefixStr, false)
		{ }

		public BinaryFileWriter(string filename, bool prefixStr, bool async)
		{
			PrefixStrings = prefixStr;

			m_Buffer = new byte[BufferSize];

			if (async)
			{
				m_File = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, true);
			}
			else
			{
				m_File = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, FileOptions.WriteThrough);
			}

			m_Encoding = Utility.UTF8WithEncoding;
		}

		public void Flush()
		{
			if (m_Index > 0)
			{
				m_Position += m_Index;

				m_File.Write(m_Buffer, 0, m_Index);
				m_Index = 0;
			}
		}

		private long m_Position;

		public override long Position => m_Position + m_Index;

		public Stream UnderlyingStream
		{
			get
			{
				if (m_Index > 0)
				{
					Flush();
				}

				return m_File;
			}
		}

		public override void Close()
		{
			if (m_Index > 0)
			{
				Flush();
			}

			m_File.Close();
		}

		public override void WriteEncodedInt(int value)
		{
			var v = (uint)value;

			while (v >= 0x80)
			{
				if ((m_Index + 1) > m_Buffer.Length)
				{
					Flush();
				}

				m_Buffer[m_Index++] = (byte)(v | 0x80);
				v >>= 7;
			}

			if ((m_Index + 1) > m_Buffer.Length)
			{
				Flush();
			}

			m_Buffer[m_Index++] = (byte)v;
		}

		private byte[] m_CharacterBuffer;
		private int m_MaxBufferChars;
		private const int LargeByteBufferSize = 256;

		internal void InternalWriteString(string value)
		{
			var length = m_Encoding.GetByteCount(value);

			WriteEncodedInt(length);

			if (m_CharacterBuffer == null)
			{
				m_CharacterBuffer = new byte[LargeByteBufferSize];
				m_MaxBufferChars = LargeByteBufferSize / m_Encoding.GetMaxByteCount(1);
			}

			if (length > LargeByteBufferSize)
			{
				var current = 0;
				var charsLeft = value.Length;

				while (charsLeft > 0)
				{
					var charCount = (charsLeft > m_MaxBufferChars) ? m_MaxBufferChars : charsLeft;
					var byteLength = m_Encoding.GetBytes(value, current, charCount, m_CharacterBuffer, 0);

					if ((m_Index + byteLength) > m_Buffer.Length)
					{
						Flush();
					}

					Buffer.BlockCopy(m_CharacterBuffer, 0, m_Buffer, m_Index, byteLength);
					m_Index += byteLength;

					current += charCount;
					charsLeft -= charCount;
				}
			}
			else
			{
				var byteLength = m_Encoding.GetBytes(value, 0, value.Length, m_CharacterBuffer, 0);

				if ((m_Index + byteLength) > m_Buffer.Length)
				{
					Flush();
				}

				Buffer.BlockCopy(m_CharacterBuffer, 0, m_Buffer, m_Index, byteLength);
				m_Index += byteLength;
			}
		}

		public override void Write(string value)
		{
			if (PrefixStrings)
			{
				if (value == null)
				{
					if ((m_Index + 1) > m_Buffer.Length)
					{
						Flush();
					}

					m_Buffer[m_Index++] = 0;
				}
				else
				{
					if ((m_Index + 1) > m_Buffer.Length)
					{
						Flush();
					}

					m_Buffer[m_Index++] = 1;

					InternalWriteString(value);
				}
			}
			else
			{
				InternalWriteString(value);
			}
		}

		public override void WriteObjectType(object value)
		{
			WriteObjectType(value?.GetType());
		}

		public override void WriteObjectType(Type value)
		{
			var hash = ScriptCompiler.FindHashByFullName(value?.FullName);

			WriteEncodedInt(hash);
		}

		public override void Write(DateTime value)
		{
			Write(value.Ticks);
		}

		public override void Write(DateTimeOffset value)
		{
			Write(value.Ticks);
			Write(value.Offset.Ticks);
		}

		public override void WriteDeltaTime(DateTime value)
		{
			var ticks = value.Ticks;
			var now = DateTime.UtcNow.Ticks;

			TimeSpan d;

			try
			{
				d = new TimeSpan(ticks - now);
			}
			catch (Exception ex)
			{
				if (ticks < now)
				{
					d = TimeSpan.MaxValue;
				}
				else
				{
					d = TimeSpan.MaxValue;
				}

				Diagnostics.ExceptionLogging.LogException(ex);
			}

			Write(d);
		}

		public override void Write(IPAddress value)
		{
			Write(Utility.GetLongAddressValue(value));
		}

		public override void Write(TimeSpan value)
		{
			Write(value.Ticks);
		}

		public override void Write(decimal value)
		{
			var bits = Decimal.GetBits(value);

			for (var i = 0; i < bits.Length; ++i)
			{
				Write(bits[i]);
			}
		}

		public override void Write(long value)
		{
			if ((m_Index + 8) > m_Buffer.Length)
			{
				Flush();
			}

			m_Buffer[m_Index] = (byte)value;
			m_Buffer[m_Index + 1] = (byte)(value >> 8);
			m_Buffer[m_Index + 2] = (byte)(value >> 16);
			m_Buffer[m_Index + 3] = (byte)(value >> 24);
			m_Buffer[m_Index + 4] = (byte)(value >> 32);
			m_Buffer[m_Index + 5] = (byte)(value >> 40);
			m_Buffer[m_Index + 6] = (byte)(value >> 48);
			m_Buffer[m_Index + 7] = (byte)(value >> 56);
			m_Index += 8;
		}

		public override void Write(ulong value)
		{
			if ((m_Index + 8) > m_Buffer.Length)
			{
				Flush();
			}

			m_Buffer[m_Index] = (byte)value;
			m_Buffer[m_Index + 1] = (byte)(value >> 8);
			m_Buffer[m_Index + 2] = (byte)(value >> 16);
			m_Buffer[m_Index + 3] = (byte)(value >> 24);
			m_Buffer[m_Index + 4] = (byte)(value >> 32);
			m_Buffer[m_Index + 5] = (byte)(value >> 40);
			m_Buffer[m_Index + 6] = (byte)(value >> 48);
			m_Buffer[m_Index + 7] = (byte)(value >> 56);
			m_Index += 8;
		}

		public override void Write(int value)
		{
			if ((m_Index + 4) > m_Buffer.Length)
			{
				Flush();
			}

			m_Buffer[m_Index] = (byte)value;
			m_Buffer[m_Index + 1] = (byte)(value >> 8);
			m_Buffer[m_Index + 2] = (byte)(value >> 16);
			m_Buffer[m_Index + 3] = (byte)(value >> 24);
			m_Index += 4;
		}

		public override void Write(uint value)
		{
			if ((m_Index + 4) > m_Buffer.Length)
			{
				Flush();
			}

			m_Buffer[m_Index] = (byte)value;
			m_Buffer[m_Index + 1] = (byte)(value >> 8);
			m_Buffer[m_Index + 2] = (byte)(value >> 16);
			m_Buffer[m_Index + 3] = (byte)(value >> 24);
			m_Index += 4;
		}

		public override void Write(short value)
		{
			if ((m_Index + 2) > m_Buffer.Length)
			{
				Flush();
			}

			m_Buffer[m_Index] = (byte)value;
			m_Buffer[m_Index + 1] = (byte)(value >> 8);
			m_Index += 2;
		}

		public override void Write(ushort value)
		{
			if ((m_Index + 2) > m_Buffer.Length)
			{
				Flush();
			}

			m_Buffer[m_Index] = (byte)value;
			m_Buffer[m_Index + 1] = (byte)(value >> 8);
			m_Index += 2;
		}

		public override unsafe void Write(double value)
		{
			if ((m_Index + 8) > m_Buffer.Length)
			{
				Flush();
			}

			fixed (byte* pBuffer = m_Buffer)
			{
				*(double*)(pBuffer + m_Index) = value;
			}

			m_Index += 8;
		}

		public override unsafe void Write(float value)
		{
			if ((m_Index + 4) > m_Buffer.Length)
			{
				Flush();
			}

			fixed (byte* pBuffer = m_Buffer)
			{
				*(float*)(pBuffer + m_Index) = value;
			}

			m_Index += 4;
		}

		private readonly char[] m_SingleCharBuffer = new char[1];

		public override void Write(char value)
		{
			if ((m_Index + 8) > m_Buffer.Length)
			{
				Flush();
			}

			m_SingleCharBuffer[0] = value;

			var byteCount = m_Encoding.GetBytes(m_SingleCharBuffer, 0, 1, m_Buffer, m_Index);
			m_Index += byteCount;
		}

		public override void Write(byte value)
		{
			if ((m_Index + 1) > m_Buffer.Length)
			{
				Flush();
			}

			m_Buffer[m_Index++] = value;
		}

		public override void Write(sbyte value)
		{
			if ((m_Index + 1) > m_Buffer.Length)
			{
				Flush();
			}

			m_Buffer[m_Index++] = (byte)value;
		}

		public override void Write(bool value)
		{
			if ((m_Index + 1) > m_Buffer.Length)
			{
				Flush();
			}

			m_Buffer[m_Index++] = (byte)(value ? 1 : 0);
		}

		public override void Write(Point3D value)
		{
			Write(value.m_X);
			Write(value.m_Y);
			Write(value.m_Z);
		}

		public override void Write(Point2D value)
		{
			Write(value.m_X);
			Write(value.m_Y);
		}

		public override void Write(Rectangle2D value)
		{
			Write(value.Start);
			Write(value.End);
		}

		public override void Write(Rectangle3D value)
		{
			Write(value.Start);
			Write(value.End);
		}

		public override void Write(Map value)
		{
			if (value != null)
			{
				Write((byte)value.MapIndex);
			}
			else
			{
				Write((byte)0xFF);
			}
		}

		public override void Write(Race value)
		{
			if (value != null)
			{
				Write((byte)value.RaceIndex);
			}
			else
			{
				Write((byte)0xFF);
			}
		}

		public override void Write(Serial value)
		{
			Write(value.Value);
		}

		public override void Write(Item value)
		{
			if (value == null || value.Deleted)
			{
				Write(Serial.MinusOne);
			}
			else
			{
				Write(value.Serial);
			}
		}

		public override void Write(Mobile value)
		{
			if (value == null || value.Deleted)
			{
				Write(Serial.MinusOne);
			}
			else
			{
				Write(value.Serial);
			}
		}

		public override void Write(BaseGuild value)
		{
			if (value == null)
			{
				Write(0);
			}
			else
			{
				Write(value.Id);
			}
		}

		public override void WriteMobileList(ArrayList list)
		{
			WriteMobileList(list, false);
		}

		public override void WriteMobileList(ArrayList list, bool tidy)
		{
			if (tidy)
			{
				for (var i = 0; i < list.Count;)
				{
					if (((Mobile)list[i]).Deleted)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (var i = 0; i < list.Count; ++i)
			{
				Write((Mobile)list[i]);
			}
		}

		public override void WriteItemList(ArrayList list)
		{
			WriteItemList(list, false);
		}

		public override void WriteItemList(ArrayList list, bool tidy)
		{
			if (tidy)
			{
				for (var i = 0; i < list.Count;)
				{
					if (((Item)list[i]).Deleted)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (var i = 0; i < list.Count; ++i)
			{
				Write((Item)list[i]);
			}
		}

		public override void WriteGuildList(ArrayList list)
		{
			WriteGuildList(list, false);
		}

		public override void WriteGuildList(ArrayList list, bool tidy)
		{
			if (tidy)
			{
				for (var i = 0; i < list.Count;)
				{
					if (((BaseGuild)list[i]).Disbanded)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (var i = 0; i < list.Count; ++i)
			{
				Write((BaseGuild)list[i]);
			}
		}

		public override void Write(List<Item> list)
		{
			Write(list, false);
		}

		public override void Write(List<Item> list, bool tidy)
		{
			if (tidy)
			{
				for (var i = 0; i < list.Count;)
				{
					if (list[i].Deleted)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (var i = 0; i < list.Count; ++i)
			{
				Write(list[i]);
			}
		}

		public override void WriteItemList<T>(List<T> list)
		{
			WriteItemList(list, false);
		}

		public override void WriteItemList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				for (var i = 0; i < list.Count;)
				{
					if (list[i].Deleted)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (var i = 0; i < list.Count; ++i)
			{
				Write(list[i]);
			}
		}

		public override void Write(HashSet<Item> set)
		{
			Write(set, false);
		}

		public override void Write(HashSet<Item> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(item => item.Deleted);
			}

			Write(set.Count);

			foreach (var item in set)
			{
				Write(item);
			}
		}

		public override void WriteItemSet<T>(HashSet<T> set)
		{
			WriteItemSet(set, false);
		}

		public override void WriteItemSet<T>(HashSet<T> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(item => item.Deleted);
			}

			Write(set.Count);

			foreach (Item item in set)
			{
				Write(item);
			}
		}

		public override void Write(List<Mobile> list)
		{
			Write(list, false);
		}

		public override void Write(List<Mobile> list, bool tidy)
		{
			if (tidy)
			{
				for (var i = 0; i < list.Count;)
				{
					if (list[i].Deleted)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (var i = 0; i < list.Count; ++i)
			{
				Write(list[i]);
			}
		}

		public override void WriteMobileList<T>(List<T> list)
		{
			WriteMobileList(list, false);
		}

		public override void WriteMobileList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				for (var i = 0; i < list.Count;)
				{
					if (list[i].Deleted)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (var i = 0; i < list.Count; ++i)
			{
				Write(list[i]);
			}
		}

		public override void Write(HashSet<Mobile> set)
		{
			Write(set, false);
		}

		public override void Write(HashSet<Mobile> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(mobile => mobile.Deleted);
			}

			Write(set.Count);

			foreach (var mob in set)
			{
				Write(mob);
			}
		}

		public override void WriteMobileSet<T>(HashSet<T> set)
		{
			WriteMobileSet(set, false);
		}

		public override void WriteMobileSet<T>(HashSet<T> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(mob => mob.Deleted);
			}

			Write(set.Count);

			foreach (Mobile mob in set)
			{
				Write(mob);
			}
		}

		public override void Write(List<BaseGuild> list)
		{
			Write(list, false);
		}

		public override void Write(List<BaseGuild> list, bool tidy)
		{
			if (tidy)
			{
				for (var i = 0; i < list.Count;)
				{
					if (list[i].Disbanded)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (var i = 0; i < list.Count; ++i)
			{
				Write(list[i]);
			}
		}

		public override void WriteGuildList<T>(List<T> list)
		{
			WriteGuildList(list, false);
		}

		public override void WriteGuildList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				for (var i = 0; i < list.Count;)
				{
					if (list[i].Disbanded)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (var i = 0; i < list.Count; ++i)
			{
				Write(list[i]);
			}
		}

		public override void Write(HashSet<BaseGuild> set)
		{
			Write(set, false);
		}

		public override void Write(HashSet<BaseGuild> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(guild => guild.Disbanded);
			}

			Write(set.Count);

			foreach (var guild in set)
			{
				Write(guild);
			}
		}

		public override void WriteGuildSet<T>(HashSet<T> set)
		{
			WriteGuildSet(set, false);
		}

		public override void WriteGuildSet<T>(HashSet<T> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(guild => guild.Disbanded);
			}

			Write(set.Count);

			foreach (BaseGuild guild in set)
			{
				Write(guild);
			}
		}
	}

	public sealed class BinaryFileReader : GenericReader
	{
		private readonly BinaryReader m_File;

		public BinaryFileReader(BinaryReader br)
		{
			m_File = br;
		}

		public void Close()
		{
			m_File.Close();
		}

		public long Position => m_File.BaseStream.Position;

		public long Seek(long offset, SeekOrigin origin)
		{
			return m_File.BaseStream.Seek(offset, origin);
		}

		public override Type ReadObjectType()
		{
			var hash = ReadEncodedInt();

			return ScriptCompiler.FindTypeByFullNameHash(hash);
		}

		public override string ReadString()
		{
			if (ReadByte() != 0)
			{
				return m_File.ReadString();
			}
			else
			{
				return null;
			}
		}

		public override DateTime ReadDeltaTime()
		{
			var offset = ReadTimeSpan();

			try
			{
				return DateTime.UtcNow + offset;
			}
			catch
			{
				if (offset <= TimeSpan.MinValue)
					return DateTime.MinValue;

				if (offset >= TimeSpan.MaxValue)
					return DateTime.MaxValue;

				return DateTime.UtcNow;
			}
		}

		public override IPAddress ReadIPAddress()
		{
			return new IPAddress(m_File.ReadInt64());
		}

		public override int ReadEncodedInt()
		{
			int v = 0, shift = 0;
			byte b;

			do
			{
				b = m_File.ReadByte();
				v |= (b & 0x7F) << shift;
				shift += 7;
			}
			while (b >= 0x80);

			return v;
		}

		public override DateTime ReadDateTime()
		{
			return new DateTime(m_File.ReadInt64());
		}

		public override DateTimeOffset ReadDateTimeOffset()
		{
			var ticks = m_File.ReadInt64();
			var offset = new TimeSpan(m_File.ReadInt64());

			return new DateTimeOffset(ticks, offset);
		}

		public override TimeSpan ReadTimeSpan()
		{
			return new TimeSpan(m_File.ReadInt64());
		}

		public override decimal ReadDecimal()
		{
			return m_File.ReadDecimal();
		}

		public override long ReadLong()
		{
			return m_File.ReadInt64();
		}

		public override ulong ReadULong()
		{
			return m_File.ReadUInt64();
		}

		public override int PeekInt()
		{
			var value = 0;
			var returnTo = m_File.BaseStream.Position;

			try
			{
				value = m_File.ReadInt32();
			}
			catch (EndOfStreamException)
			{
				// Ignore this exception, the defalut value 0 will be returned
			}

			m_File.BaseStream.Seek(returnTo, SeekOrigin.Begin);
			return value;
		}

		public override int ReadInt()
		{
			return m_File.ReadInt32();
		}

		public override uint ReadUInt()
		{
			return m_File.ReadUInt32();
		}

		public override short ReadShort()
		{
			return m_File.ReadInt16();
		}

		public override ushort ReadUShort()
		{
			return m_File.ReadUInt16();
		}

		public override double ReadDouble()
		{
			return m_File.ReadDouble();
		}

		public override float ReadFloat()
		{
			return m_File.ReadSingle();
		}

		public override char ReadChar()
		{
			return m_File.ReadChar();
		}

		public override byte ReadByte()
		{
			return m_File.ReadByte();
		}

		public override sbyte ReadSByte()
		{
			return m_File.ReadSByte();
		}

		public override bool ReadBool()
		{
			return m_File.ReadBoolean();
		}

		public override Point3D ReadPoint3D()
		{
			return new Point3D(ReadInt(), ReadInt(), ReadInt());
		}

		public override Point2D ReadPoint2D()
		{
			return new Point2D(ReadInt(), ReadInt());
		}

		public override Rectangle2D ReadRect2D()
		{
			return new Rectangle2D(ReadPoint2D(), ReadPoint2D());
		}

		public override Rectangle3D ReadRect3D()
		{
			return new Rectangle3D(ReadPoint3D(), ReadPoint3D());
		}

		public override Map ReadMap()
		{
			return Map.Maps[ReadByte()];
		}

		public override Serial ReadSerial()
		{
			return new Serial(ReadInt());
		}

		public override Item ReadItem()
		{
			return World.FindItem(ReadSerial());
		}

		public override Mobile ReadMobile()
		{
			return World.FindMobile(ReadSerial());
		}

		public override BaseGuild ReadGuild()
		{
			return BaseGuild.Find(ReadInt());
		}

		public override T ReadItem<T>()
		{
			return ReadItem() as T;
		}

		public override T ReadMobile<T>()
		{
			return ReadMobile() as T;
		}

		public override T ReadGuild<T>()
		{
			return ReadGuild() as T;
		}

		public override ArrayList ReadItemList()
		{
			var count = ReadInt();

			if (count > 0)
			{
				var list = new ArrayList(count);

				for (var i = 0; i < count; ++i)
				{
					var item = ReadItem();

					if (item != null)
					{
						list.Add(item);
					}
				}

				return list;
			}
			else
			{
				return new ArrayList();
			}
		}

		public override ArrayList ReadMobileList()
		{
			var count = ReadInt();

			if (count > 0)
			{
				var list = new ArrayList(count);

				for (var i = 0; i < count; ++i)
				{
					var m = ReadMobile();

					if (m != null)
					{
						list.Add(m);
					}
				}

				return list;
			}
			else
			{
				return new ArrayList();
			}
		}

		public override ArrayList ReadGuildList()
		{
			var count = ReadInt();

			if (count > 0)
			{
				var list = new ArrayList(count);

				for (var i = 0; i < count; ++i)
				{
					var g = ReadGuild();

					if (g != null)
					{
						list.Add(g);
					}
				}

				return list;
			}
			else
			{
				return new ArrayList();
			}
		}

		public override List<Item> ReadStrongItemList()
		{
			return ReadStrongItemList<Item>();
		}

		public override List<T> ReadStrongItemList<T>()
		{
			var count = ReadInt();

			if (count > 0)
			{
				var list = new List<T>(count);

				for (var i = 0; i < count; ++i)
				{
					if (ReadItem() is T item)
					{
						list.Add(item);
					}
				}

				return list;
			}
			else
			{
				return new List<T>();
			}
		}

		public override HashSet<Item> ReadItemSet()
		{
			return ReadItemSet<Item>();
		}

		public override HashSet<T> ReadItemSet<T>()
		{
			var count = ReadInt();

			if (count > 0)
			{
				var set = new HashSet<T>();

				for (var i = 0; i < count; ++i)
				{
					if (ReadItem() is T item)
					{
						set.Add(item);
					}
				}

				return set;
			}
			else
			{
				return new HashSet<T>();
			}
		}

		public override List<Mobile> ReadStrongMobileList()
		{
			return ReadStrongMobileList<Mobile>();
		}

		public override List<T> ReadStrongMobileList<T>()
		{
			var count = ReadInt();

			if (count > 0)
			{
				var list = new List<T>(count);

				for (var i = 0; i < count; ++i)
				{
					if (ReadMobile() is T m)
					{
						list.Add(m);
					}
				}

				return list;
			}
			else
			{
				return new List<T>();
			}
		}

		public override HashSet<Mobile> ReadMobileSet()
		{
			return ReadMobileSet<Mobile>();
		}

		public override HashSet<T> ReadMobileSet<T>()
		{
			var count = ReadInt();

			if (count > 0)
			{
				var set = new HashSet<T>();

				for (var i = 0; i < count; ++i)
				{
					if (ReadMobile() is T m)
					{
						set.Add(m);
					}
				}

				return set;
			}
			else
			{
				return new HashSet<T>();
			}
		}

		public override List<BaseGuild> ReadStrongGuildList()
		{
			return ReadStrongGuildList<BaseGuild>();
		}

		public override List<T> ReadStrongGuildList<T>()
		{
			var count = ReadInt();

			if (count > 0)
			{
				var list = new List<T>(count);

				for (var i = 0; i < count; ++i)
				{
					if (ReadGuild() is T g)
					{
						list.Add(g);
					}
				}

				return list;
			}
			else
			{
				return new List<T>();
			}
		}

		public override HashSet<BaseGuild> ReadGuildSet()
		{
			return ReadGuildSet<BaseGuild>();
		}

		public override HashSet<T> ReadGuildSet<T>()
		{
			var count = ReadInt();

			if (count > 0)
			{
				var set = new HashSet<T>();

				for (var i = 0; i < count; ++i)
				{
					if (ReadGuild() is T g)
					{
						set.Add(g);
					}
				}

				return set;
			}
			else
			{
				return new HashSet<T>();
			}
		}

		public override Race ReadRace()
		{
			return Race.Races[ReadByte()];
		}

		public override bool End()
		{
			return m_File.PeekChar() == -1;
		}
	}

	public sealed class AsyncWriter : GenericWriter
	{
		public static int ThreadCount { get; private set; }

		private readonly int BufferSize;

		private long m_LastPos, m_CurPos;
		private bool m_Closed;
		private readonly bool PrefixStrings;

		private MemoryStream m_Mem;
		private BinaryWriter m_Bin;
		private readonly FileStream m_File;

		private readonly Queue m_WriteQueue;
		private Thread m_WorkerThread;

		public AsyncWriter(string filename, bool prefix)
			: this(filename, 1048576, prefix) //1 mb buffer
		{ }

		public AsyncWriter(string filename, int buffSize, bool prefix)
		{
			PrefixStrings = prefix;
			m_Closed = false;
			m_WriteQueue = Queue.Synchronized(new Queue());
			BufferSize = buffSize;

			m_File = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, FileOptions.Asynchronous);
			m_Mem = new MemoryStream(BufferSize + 1024);
			m_Bin = new BinaryWriter(m_Mem, Utility.UTF8WithEncoding);
		}

		private void Enqueue(MemoryStream mem)
		{
			m_WriteQueue.Enqueue(mem);

			if (m_WorkerThread == null || !m_WorkerThread.IsAlive)
			{
				m_WorkerThread = new Thread(new WorkerThread(this).Worker)
				{
					Priority = ThreadPriority.BelowNormal
				};
				m_WorkerThread.Start();
			}
		}

		private class WorkerThread
		{
			private readonly AsyncWriter m_Owner;

			public WorkerThread(AsyncWriter owner)
			{
				m_Owner = owner;
			}

			public void Worker()
			{
				ThreadCount++;

				while (m_Owner.m_WriteQueue.Count > 0)
				{
					var mem = (MemoryStream)m_Owner.m_WriteQueue.Dequeue();

					if (mem != null && mem.Length > 0)
					{
						mem.WriteTo(m_Owner.m_File);
					}
				}

				if (m_Owner.m_Closed)
				{
					m_Owner.m_File.Close();
				}

				if (--ThreadCount <= 0)
				{
					World.NotifyDiskWriteComplete();
				}
			}
		}

		private void OnWrite()
		{
			var curlen = m_Mem.Length;
			m_CurPos += curlen - m_LastPos;
			m_LastPos = curlen;
			if (curlen >= BufferSize)
			{
				Enqueue(m_Mem);
				m_Mem = new MemoryStream(BufferSize + 1024);
				m_Bin = new BinaryWriter(m_Mem, Utility.UTF8WithEncoding);
				m_LastPos = 0;
			}
		}

		public MemoryStream MemStream
		{
			get => m_Mem;
			set
			{
				if (m_Mem.Length > 0)
				{
					Enqueue(m_Mem);
				}

				m_Mem = value;
				m_Bin = new BinaryWriter(m_Mem, Utility.UTF8WithEncoding);
				m_LastPos = 0;
				m_CurPos = m_Mem.Length;
				m_Mem.Seek(0, SeekOrigin.End);
			}
		}

		public override void Close()
		{
			Enqueue(m_Mem);
			m_Closed = true;
		}

		public override long Position => m_CurPos;

		public override void WriteObjectType(object value)
		{
			WriteObjectType(value?.GetType());
		}

		public override void WriteObjectType(Type value)
		{
			var hash = ScriptCompiler.FindHashByFullName(value?.FullName);

			WriteEncodedInt(hash);
		}

		public override void Write(IPAddress value)
		{
			m_Bin.Write(Utility.GetLongAddressValue(value));
			OnWrite();
		}

		public override void Write(string value)
		{
			if (PrefixStrings)
			{
				if (value == null)
				{
					m_Bin.Write((byte)0);
				}
				else
				{
					m_Bin.Write((byte)1);
					m_Bin.Write(value);
				}
			}
			else
			{
				m_Bin.Write(value);
			}
			OnWrite();
		}

		public override void WriteDeltaTime(DateTime value)
		{
			var ticks = value.Ticks;
			var now = DateTime.UtcNow.Ticks;

			TimeSpan d;

			try
			{
				d = new TimeSpan(ticks - now);
			}
			catch (Exception ex)
			{
				if (ticks < now)
				{
					d = TimeSpan.MaxValue;
				}
				else
				{
					d = TimeSpan.MaxValue;
				}

				Diagnostics.ExceptionLogging.LogException(ex);
			}

			Write(d);
		}

		public override void Write(DateTime value)
		{
			m_Bin.Write(value.Ticks);
			OnWrite();
		}

		public override void Write(DateTimeOffset value)
		{
			m_Bin.Write(value.Ticks);
			m_Bin.Write(value.Offset.Ticks);
			OnWrite();
		}

		public override void Write(TimeSpan value)
		{
			m_Bin.Write(value.Ticks);
			OnWrite();
		}

		public override void Write(decimal value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(long value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(ulong value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void WriteEncodedInt(int value)
		{
			var v = (uint)value;

			while (v >= 0x80)
			{
				m_Bin.Write((byte)(v | 0x80));
				v >>= 7;
			}

			m_Bin.Write((byte)v);
			OnWrite();
		}

		public override void Write(int value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(uint value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(short value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(ushort value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(double value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(float value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(char value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(byte value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(sbyte value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(bool value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(Point3D value)
		{
			Write(value.m_X);
			Write(value.m_Y);
			Write(value.m_Z);
		}

		public override void Write(Point2D value)
		{
			Write(value.m_X);
			Write(value.m_Y);
		}

		public override void Write(Rectangle2D value)
		{
			Write(value.Start);
			Write(value.End);
		}

		public override void Write(Rectangle3D value)
		{
			Write(value.Start);
			Write(value.End);
		}

		public override void Write(Map value)
		{
			if (value != null)
			{
				Write((byte)value.MapIndex);
			}
			else
			{
				Write((byte)0xFF);
			}
		}

		public override void Write(Race value)
		{
			if (value != null)
			{
				Write((byte)value.RaceIndex);
			}
			else
			{
				Write((byte)0xFF);
			}
		}

		public override void Write(Serial value)
		{
			Write(value.Value);
		}

		public override void Write(Item value)
		{
			if (value == null || value.Deleted)
			{
				Write(Serial.MinusOne);
			}
			else
			{
				Write(value.Serial);
			}
		}

		public override void Write(Mobile value)
		{
			if (value == null || value.Deleted)
			{
				Write(Serial.MinusOne);
			}
			else
			{
				Write(value.Serial);
			}
		}

		public override void Write(BaseGuild value)
		{
			if (value == null)
			{
				Write(0);
			}
			else
			{
				Write(value.Id);
			}
		}

		public override void WriteMobileList(ArrayList list)
		{
			WriteMobileList(list, false);
		}

		public override void WriteMobileList(ArrayList list, bool tidy)
		{
			if (tidy)
			{
				for (var i = 0; i < list.Count;)
				{
					if (((Mobile)list[i]).Deleted)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (var i = 0; i < list.Count; ++i)
			{
				Write((Mobile)list[i]);
			}
		}

		public override void WriteItemList(ArrayList list)
		{
			WriteItemList(list, false);
		}

		public override void WriteItemList(ArrayList list, bool tidy)
		{
			if (tidy)
			{
				for (var i = 0; i < list.Count;)
				{
					if (((Item)list[i]).Deleted)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (var i = 0; i < list.Count; ++i)
			{
				Write((Item)list[i]);
			}
		}

		public override void WriteGuildList(ArrayList list)
		{
			WriteGuildList(list, false);
		}

		public override void WriteGuildList(ArrayList list, bool tidy)
		{
			if (tidy)
			{
				for (var i = 0; i < list.Count;)
				{
					if (((BaseGuild)list[i]).Disbanded)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (var i = 0; i < list.Count; ++i)
			{
				Write((BaseGuild)list[i]);
			}
		}

		public override void Write(List<Item> list)
		{
			Write(list, false);
		}

		public override void Write(List<Item> list, bool tidy)
		{
			if (tidy)
			{
				for (var i = 0; i < list.Count;)
				{
					if (list[i].Deleted)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (var i = 0; i < list.Count; ++i)
			{
				Write(list[i]);
			}
		}

		public override void WriteItemList<T>(List<T> list)
		{
			WriteItemList(list, false);
		}

		public override void WriteItemList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				for (var i = 0; i < list.Count;)
				{
					if (list[i].Deleted)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (var i = 0; i < list.Count; ++i)
			{
				Write(list[i]);
			}
		}

		public override void Write(HashSet<Item> set)
		{
			Write(set, false);
		}

		public override void Write(HashSet<Item> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(item => item.Deleted);
			}

			Write(set.Count);

			foreach (var item in set)
			{
				Write(item);
			}
		}

		public override void WriteItemSet<T>(HashSet<T> set)
		{
			WriteItemSet(set, false);
		}

		public override void WriteItemSet<T>(HashSet<T> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(item => item.Deleted);
			}

			Write(set.Count);

			foreach (Item item in set)
			{
				Write(item);
			}
		}

		public override void Write(List<Mobile> list)
		{
			Write(list, false);
		}

		public override void Write(List<Mobile> list, bool tidy)
		{
			if (tidy)
			{
				for (var i = 0; i < list.Count;)
				{
					if (list[i].Deleted)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (var i = 0; i < list.Count; ++i)
			{
				Write(list[i]);
			}
		}

		public override void WriteMobileList<T>(List<T> list)
		{
			WriteMobileList(list, false);
		}

		public override void WriteMobileList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				for (var i = 0; i < list.Count;)
				{
					if (list[i].Deleted)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (var i = 0; i < list.Count; ++i)
			{
				Write(list[i]);
			}
		}

		public override void Write(HashSet<Mobile> set)
		{
			Write(set, false);
		}

		public override void Write(HashSet<Mobile> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(mobile => mobile.Deleted);
			}

			Write(set.Count);

			foreach (var mob in set)
			{
				Write(mob);
			}
		}

		public override void WriteMobileSet<T>(HashSet<T> set)
		{
			WriteMobileSet(set, false);
		}

		public override void WriteMobileSet<T>(HashSet<T> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(mob => mob.Deleted);
			}

			Write(set.Count);

			foreach (Mobile mob in set)
			{
				Write(mob);
			}
		}

		public override void Write(List<BaseGuild> list)
		{
			Write(list, false);
		}

		public override void Write(List<BaseGuild> list, bool tidy)
		{
			if (tidy)
			{
				for (var i = 0; i < list.Count;)
				{
					if (list[i].Disbanded)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (var i = 0; i < list.Count; ++i)
			{
				Write(list[i]);
			}
		}

		public override void WriteGuildList<T>(List<T> list)
		{
			WriteGuildList(list, false);
		}

		public override void WriteGuildList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				for (var i = 0; i < list.Count;)
				{
					if (list[i].Disbanded)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (var i = 0; i < list.Count; ++i)
			{
				Write(list[i]);
			}
		}

		public override void Write(HashSet<BaseGuild> set)
		{
			Write(set, false);
		}

		public override void Write(HashSet<BaseGuild> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(guild => guild.Disbanded);
			}

			Write(set.Count);

			foreach (var guild in set)
			{
				Write(guild);
			}
		}

		public override void WriteGuildSet<T>(HashSet<T> set)
		{
			WriteGuildSet(set, false);
		}

		public override void WriteGuildSet<T>(HashSet<T> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(guild => guild.Disbanded);
			}

			Write(set.Count);

			foreach (BaseGuild guild in set)
			{
				Write(guild);
			}
		}
	}

	public interface ISerializable
	{
		int TypeReference { get; }
		int SerialIdentity { get; }

		void Serialize(GenericWriter writer);
	}
}
