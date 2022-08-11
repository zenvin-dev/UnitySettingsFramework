using System;
using System.Text;

namespace Zenvin.Settings.Framework.Serialization {
	public partial class ValuePacket {

		public bool TryRead (string key, out sbyte value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (sbyte)) {
				value = (sbyte)raw[0];
				return true;
			}
			value = default;
			return false;
		}

		public bool TryRead (string key, out byte value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (byte)) {
				value = raw[0];
				return true;
			}
			value = default;
			return false;
		}

		public bool TryRead (string key, out short value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (short)) {
				value = BitConverter.ToInt16 (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		public bool TryRead (string key, out ushort value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (ushort)) {
				value = BitConverter.ToUInt16 (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		public bool TryRead (string key, out int value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (int)) {
				value = BitConverter.ToInt32 (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		public bool TryRead (string key, out uint value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (uint)) {
				value = BitConverter.ToUInt32 (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		public bool TryRead (string key, out long value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (long)) {
				value = BitConverter.ToInt64 (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		public bool TryRead (string key, out ulong value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (ulong)) {
				value = BitConverter.ToUInt64 (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		public bool TryRead (string key, out float value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (float)) {
				value = BitConverter.ToSingle (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		public bool TryRead (string key, out double value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (double)) {
				value = BitConverter.ToDouble (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		public bool TryRead (string key, out bool value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (bool)) {
				value = BitConverter.ToBoolean (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		public bool TryRead (string key, out string value, Encoding encoding = null) {
			encoding = encoding ?? Encoding.UTF8;
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length > 0) {
				value = encoding.GetString (raw);
				return true;
			}
			value = "";
			return false;
		}

		public bool TryRead (string key, out char value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (char)) {
				value = BitConverter.ToChar (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		public bool TryRead (string key, out byte[] value) {
			return data.TryGetValue (key, out value);
		}


		public sbyte ReadSByte (string key) {
			if (TryRead (key, out sbyte value)) {
				return value;
			}
			throw new Exception ($"Could not read SByte value '{key}'");
		}

		public byte ReadByte (string key) {
			if (TryRead (key, out byte value)) {
				return value;
			}
			throw new Exception ($"Could not read Byte value '{key}'");
		}

		public short ReadInt16 (string key) {
			if (TryRead (key, out short value)) {
				return value;
			}
			throw new Exception ($"Could not read Int16 value '{key}'");
		}

		public ushort ReadUInt16 (string key) {
			if (TryRead (key, out ushort value)) {
				return value;
			}
			throw new Exception ($"Could not read UInt16 value '{key}'");
		}

		public int ReadInt32 (string key) {
			if (TryRead (key, out int value)) {
				return value;
			}
			throw new Exception ($"Could not read Int32 value '{key}'");
		}

		public uint ReadUInt32 (string key) {
			if (TryRead (key, out uint value)) {
				return value;
			}
			throw new Exception ($"Could not read UInt32 value '{key}'");
		}

		public long ReadInt64 (string key) {
			if (TryRead (key, out long value)) {
				return value;
			}
			throw new Exception ($"Could not read Int64 value '{key}'");
		}

		public ulong ReadUInt64 (string key) {
			if (TryRead (key, out ulong value)) {
				return value;
			}
			throw new Exception ($"Could not read UInt64 value '{key}'");
		}

		public float ReadSingle (string key) {
			if (TryRead (key, out float value)) {
				return value;
			}
			throw new Exception ($"Could not read Single value '{key}'");
		}

		public double ReadDouble (string key) {
			if (TryRead (key, out double value)) {
				return value;
			}
			throw new Exception ($"Could not read Double value '{key}'");
		}

		public bool ReadBoolean (string key) {
			if (TryRead (key, out bool value)) {
				return value;
			}
			throw new Exception ($"Could not read Boolean value '{key}'");
		}

		public char ReadChar (string key) {
			if (TryRead (key, out char value)) {
				return value;
			}
			throw new Exception ($"Could not read Char value '{key}'");
		}

		public string ReadString (string key) {
			if (TryRead (key, out string value)) {
				return value;
			}
			throw new Exception ($"Could not read String value '{key}'");
		}

		public byte[] ReadBytes (string key) {
			if (TryRead (key, out byte[] value)) {
				return value;
			}
			throw new Exception ($"Could not read Byte[] value '{key}'");
		}


		public sbyte Read (string key, sbyte fallback) {
			if (TryRead (key, out sbyte value)) {
				return value;
			}
			return fallback;
		}

		public byte Read (string key, byte fallback) {
			if (TryRead (key, out byte value)) {
				return value;
			}
			return fallback;
		}

		public short Read (string key, short fallback) {
			if (TryRead (key, out short value)) {
				return value;
			}
			return fallback;
		}

		public ushort Read (string key, ushort fallback) {
			if (TryRead (key, out ushort value)) {
				return value;
			}
			return fallback;
		}

		public int Read (string key, int fallback) {
			if (TryRead (key, out int value)) {
				return value;
			}
			return fallback;
		}

		public uint Read (string key, uint fallback) {
			if (TryRead (key, out uint value)) {
				return value;
			}
			return fallback;
		}

		public long Read (string key, long fallback) {
			if (TryRead (key, out long value)) {
				return value;
			}
			return fallback;
		}

		public ulong Read (string key, ulong fallback) {
			if (TryRead (key, out ulong value)) {
				return value;
			}
			return fallback;
		}

		public float Read (string key, float fallback) {
			if (TryRead (key, out float value)) {
				return value;
			}
			return fallback;
		}

		public double Read (string key, double fallback) {
			if (TryRead (key, out double value)) {
				return value;
			}
			return fallback;
		}

		public bool Read (string key, bool fallback) {
			if (TryRead (key, out bool value)) {
				return value;
			}
			return fallback;
		}

		public char Read (string key, char fallback) {
			if (TryRead (key, out char value)) {
				return value;
			}
			return fallback;
		}

		public string Read (string key, string fallback) {
			if (TryRead (key, out string value)) {
				return value;
			}
			return fallback;
		}

		public byte[] Read (string key, byte[] fallback) {
			if (TryRead (key, out byte[] value)) {
				return value;
			}
			return fallback;
		}

	}
}