using System;
using System.Collections.Generic;
using System.Text;

namespace Zenvin.Settings.Framework.Serialization {
	public partial class ValuePacket {

		/// <summary>
		/// Tries to read an <see cref="sbyte"/> value with the given key.<br></br>
		/// Returns <see langword="false"/> if the key was not found, or the value associated with the key was not an <see cref="sbyte"/>.
		/// </summary>
		public bool TryRead (string key, out sbyte value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (sbyte)) {
				value = (sbyte)raw[0];
				return true;
			}
			value = default;
			return false;
		}

		/// <summary>
		/// Tries to read a <see cref="byte"/> value with the given key.<br></br>
		/// Returns <see langword="false"/> if the key was not found, or the value associated with the key was not a <see cref="byte"/>.
		/// </summary>
		public bool TryRead (string key, out byte value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (byte)) {
				value = raw[0];
				return true;
			}
			value = default;
			return false;
		}

		/// <summary>
		/// Tries to read a <see cref="short"/> value with the given key.<br></br>
		/// Returns <see langword="false"/> if the key was not found, or the value associated with the key was not a <see cref="short"/>.
		/// </summary>
		public bool TryRead (string key, out short value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (short)) {
				value = BitConverter.ToInt16 (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		/// <summary>
		/// Tries to read an <see cref="ushort"/> value with the given key.<br></br>
		/// Returns <see langword="false"/> if the key was not found, or the value associated with the key was not an <see cref="ushort"/>.
		/// </summary>
		public bool TryRead (string key, out ushort value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (ushort)) {
				value = BitConverter.ToUInt16 (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		/// <summary>
		/// Tries to read an <see cref="int"/> value with the given key.<br></br>
		/// Returns <see langword="false"/> if the key was not found, or the value associated with the key was not an <see cref="int"/>.
		/// </summary>
		public bool TryRead (string key, out int value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (int)) {
				value = BitConverter.ToInt32 (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		/// <summary>
		/// Tries to read an <see cref="uint"/> value with the given key.<br></br>
		/// Returns <see langword="false"/> if the key was not found, or the value associated with the key was not an <see cref="uint"/>.
		/// </summary>
		public bool TryRead (string key, out uint value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (uint)) {
				value = BitConverter.ToUInt32 (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		/// <summary>
		/// Tries to read a <see cref="long"/> value with the given key.<br></br>
		/// Returns <see langword="false"/> if the key was not found, or the value associated with the key was not a <see cref="long"/>.
		/// </summary>
		public bool TryRead (string key, out long value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (long)) {
				value = BitConverter.ToInt64 (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		/// <summary>
		/// Tries to read an <see cref="ulong"/> value with the given key.<br></br>
		/// Returns <see langword="false"/> if the key was not found, or the value associated with the key was not an <see cref="ulong"/>.
		/// </summary>
		public bool TryRead (string key, out ulong value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (ulong)) {
				value = BitConverter.ToUInt64 (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		/// <summary>
		/// Tries to read a <see cref="float"/> value with the given key.<br></br>
		/// Returns <see langword="false"/> if the key was not found, or the value associated with the key was not a <see cref="float"/>.
		/// </summary>
		public bool TryRead (string key, out float value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (float)) {
				value = BitConverter.ToSingle (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		/// <summary>
		/// Tries to read a <see cref="double"/> value with the given key.<br></br>
		/// Returns <see langword="false"/> if the key was not found, or the value associated with the key was not a <see cref="double"/>.
		/// </summary>
		public bool TryRead (string key, out double value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (double)) {
				value = BitConverter.ToDouble (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		/// <summary>
		/// Tries to read a <see cref="bool"/> value with the given key.<br></br>
		/// Returns <see langword="false"/> if the key was not found, or the value associated with the key was not a <see cref="bool"/>.
		/// </summary>
		public bool TryRead (string key, out bool value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (bool)) {
				value = BitConverter.ToBoolean (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		/// <summary>
		/// Tries to read a <see cref="string"/> value with the given key and <see cref="Encoding"/>.<br></br>
		/// If no encoding is given, <see cref="Encoding.UTF8"/> will be used.<br></br>
		/// Returns <see langword="false"/> if the key was not found, or the value associated with the key was empty.
		/// </summary>
		public bool TryRead (string key, out string value, Encoding encoding = null) {
			encoding = encoding ?? Encoding.UTF8;
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length > 0) {
				value = encoding.GetString (raw);
				return true;
			}
			value = "";
			return false;
		}

		/// <summary>
		/// Tries to read a <see cref="char"/> value with the given key.<br></br>
		/// Returns <see langword="false"/> if the key was not found, or the value associated with the key was not a <see cref="char"/>.
		/// </summary>
		public bool TryRead (string key, out char value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (char)) {
				value = BitConverter.ToChar (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		/// <summary>
		/// Tries to read a <see cref="byte"/>[] value with the given key.<br></br>
		/// Returns <see langword="false"/> if the key was not found.<br></br>
		/// Returned value may be <see langword="null"/>.
		/// </summary>
		public bool TryRead (string key, out byte[] value) {
			return data.TryGetValue (key, out value);
		}


		/// <summary>
		/// Reads an <see cref="sbyte"/> value with the given key.
		/// </summary>
		/// <exception cref="KeyNotFoundException"/>
		public sbyte ReadSByte (string key) {
			if (TryRead (key, out sbyte value)) {
				return value;
			}
			throw new KeyNotFoundException ($"Could not read SByte value '{key}'");
		}

		/// <summary>
		/// Reads a <see cref="byte"/> value with the given key.
		/// </summary>
		/// <exception cref="KeyNotFoundException"/>
		public byte ReadByte (string key) {
			if (TryRead (key, out byte value)) {
				return value;
			}
			throw new KeyNotFoundException ($"Could not read Byte value '{key}'");
		}

		/// <summary>
		/// Reads a <see cref="short"/> value with the given key.
		/// </summary>
		/// <exception cref="KeyNotFoundException"/>
		public short ReadInt16 (string key) {
			if (TryRead (key, out short value)) {
				return value;
			}
			throw new KeyNotFoundException ($"Could not read Int16 value '{key}'");
		}

		/// <summary>
		/// Reads an <see cref="ushort"/> value with the given key.
		/// </summary>
		/// <exception cref="KeyNotFoundException"/>
		public ushort ReadUInt16 (string key) {
			if (TryRead (key, out ushort value)) {
				return value;
			}
			throw new KeyNotFoundException ($"Could not read UInt16 value '{key}'");
		}

		/// <summary>
		/// Reads an <see cref="int"/> value with the given key.
		/// </summary>
		/// <exception cref="KeyNotFoundException"/>
		public int ReadInt32 (string key) {
			if (TryRead (key, out int value)) {
				return value;
			}
			throw new KeyNotFoundException ($"Could not read Int32 value '{key}'");
		}

		/// <summary>
		/// Reads an <see cref="uint"/> value with the given key.
		/// </summary>
		/// <exception cref="KeyNotFoundException"/>
		public uint ReadUInt32 (string key) {
			if (TryRead (key, out uint value)) {
				return value;
			}
			throw new KeyNotFoundException ($"Could not read UInt32 value '{key}'");
		}

		/// <summary>
		/// Reads a <see cref="long"/> value with the given key.
		/// </summary>
		/// <exception cref="KeyNotFoundException"/>
		public long ReadInt64 (string key) {
			if (TryRead (key, out long value)) {
				return value;
			}
			throw new KeyNotFoundException ($"Could not read Int64 value '{key}'");
		}

		/// <summary>
		/// Reads an <see cref="ulong"/> value with the given key.
		/// </summary>
		/// <exception cref="KeyNotFoundException"/>
		public ulong ReadUInt64 (string key) {
			if (TryRead (key, out ulong value)) {
				return value;
			}
			throw new KeyNotFoundException ($"Could not read UInt64 value '{key}'");
		}

		/// <summary>
		/// Reads a <see cref="float"/> value with the given key.
		/// </summary>
		/// <exception cref="KeyNotFoundException"/>
		public float ReadSingle (string key) {
			if (TryRead (key, out float value)) {
				return value;
			}
			throw new KeyNotFoundException ($"Could not read Single value '{key}'");
		}

		/// <summary>
		/// Reads a <see cref="double"/> value with the given key.
		/// </summary>
		/// <exception cref="KeyNotFoundException"/>
		public double ReadDouble (string key) {
			if (TryRead (key, out double value)) {
				return value;
			}
			throw new KeyNotFoundException ($"Could not read Double value '{key}'");
		}

		/// <summary>
		/// Reads a <see cref="bool"/> value with the given key.
		/// </summary>
		/// <exception cref="KeyNotFoundException"/>
		public bool ReadBoolean (string key) {
			if (TryRead (key, out bool value)) {
				return value;
			}
			throw new KeyNotFoundException ($"Could not read Boolean value '{key}'");
		}

		/// <summary>
		/// Reads a <see cref="char"/> value with the given key.
		/// </summary>
		/// <exception cref="KeyNotFoundException"/>
		public char ReadChar (string key) {
			if (TryRead (key, out char value)) {
				return value;
			}
			throw new KeyNotFoundException ($"Could not read Char value '{key}'");
		}

		/// <summary>
		/// Reads a <see cref="string"/> value with the given key and <see cref="Encoding"/>.<br></br>
		/// If no encoding is given, <see cref="Encoding.UTF8"/> will be used.
		/// </summary>
		/// <exception cref="KeyNotFoundException"/>
		public string ReadString (string key, Encoding encoding = null) {
			if (TryRead (key, out string value, encoding)) {
				return value;
			}
			throw new KeyNotFoundException ($"Could not read String value '{key}'");
		}

		/// <summary>
		/// Reads a <see cref="byte"/>[] value with the given key.
		/// </summary>
		/// <exception cref="KeyNotFoundException"/>
		public byte[] ReadBytes (string key) {
			if (TryRead (key, out byte[] value)) {
				return value;
			}
			throw new KeyNotFoundException ($"Could not read Byte[] value '{key}'");
		}


		/// <summary>
		/// Reads an <see cref="sbyte"/> value with the given key.<br></br>
		/// Returns <paramref name="fallback"/>, if the key was not found or the associated value was not an <see cref="sbyte"/>.
		/// </summary>
		public sbyte Read (string key, sbyte fallback) {
			if (TryRead (key, out sbyte value)) {
				return value;
			}
			return fallback;
		}

		/// <summary>
		/// Reads a <see cref="byte"/> value with the given key.<br></br>
		/// Returns <paramref name="fallback"/>, if the key was not found or the associated value was not a <see cref="byte"/>.
		/// </summary>
		public byte Read (string key, byte fallback) {
			if (TryRead (key, out byte value)) {
				return value;
			}
			return fallback;
		}

		/// <summary>
		/// Reads a <see cref="short"/> value with the given key.<br></br>
		/// Returns <paramref name="fallback"/>, if the key was not found or the associated value was not a <see cref="short"/>.
		/// </summary>
		public short Read (string key, short fallback) {
			if (TryRead (key, out short value)) {
				return value;
			}
			return fallback;
		}

		/// <summary>
		/// Reads an <see cref="ushort"/> value with the given key.<br></br>
		/// Returns <paramref name="fallback"/>, if the key was not found or the associated value was not an <see cref="ushort"/>.
		/// </summary>
		public ushort Read (string key, ushort fallback) {
			if (TryRead (key, out ushort value)) {
				return value;
			}
			return fallback;
		}

		/// <summary>
		/// Reads an <see cref="int"/> value with the given key.<br></br>
		/// Returns <paramref name="fallback"/>, if the key was not found or the associated value was not an <see cref="int"/>.
		/// </summary>
		public int Read (string key, int fallback) {
			if (TryRead (key, out int value)) {
				return value;
			}
			return fallback;
		}

		/// <summary>
		/// Reads an <see cref="uint"/> value with the given key.<br></br>
		/// Returns <paramref name="fallback"/>, if the key was not found or the associated value was not an <see cref="uint"/>.
		/// </summary>
		public uint Read (string key, uint fallback) {
			if (TryRead (key, out uint value)) {
				return value;
			}
			return fallback;
		}

		/// <summary>
		/// Reads a <see cref="long"/> value with the given key.<br></br>
		/// Returns <paramref name="fallback"/>, if the key was not found or the associated value was not a <see cref="long"/>.
		/// </summary>
		public long Read (string key, long fallback) {
			if (TryRead (key, out long value)) {
				return value;
			}
			return fallback;
		}

		/// <summary>
		/// Reads an <see cref="ulong"/> value with the given key.<br></br>
		/// Returns <paramref name="fallback"/>, if the key was not found or the associated value was not an <see cref="ulong"/>.
		/// </summary>
		public ulong Read (string key, ulong fallback) {
			if (TryRead (key, out ulong value)) {
				return value;
			}
			return fallback;
		}

		/// <summary>
		/// Reads an <see cref="float"/> value with the given key.<br></br>
		/// Returns <paramref name="fallback"/>, if the key was not found or the associated value was not an <see cref="float"/>.
		/// </summary>
		public float Read (string key, float fallback) {
			if (TryRead (key, out float value)) {
				return value;
			}
			return fallback;
		}

		/// <summary>
		/// Reads a <see cref="double"/> value with the given key.<br></br>
		/// Returns <paramref name="fallback"/>, if the key was not found or the associated value was not a <see cref="double"/>.
		/// </summary>
		public double Read (string key, double fallback) {
			if (TryRead (key, out double value)) {
				return value;
			}
			return fallback;
		}

		/// <summary>
		/// Reads a <see cref="bool"/> value with the given key.<br></br>
		/// Returns <paramref name="fallback"/>, if the key was not found or the associated value was not a <see cref="bool"/>.
		/// </summary>
		public bool Read (string key, bool fallback) {
			if (TryRead (key, out bool value)) {
				return value;
			}
			return fallback;
		}

		/// <summary>
		/// Reads a <see cref="char"/> value with the given key.<br></br>
		/// Returns <paramref name="fallback"/>, if the key was not found or the associated value was not a <see cref="char"/>.
		/// </summary>
		public char Read (string key, char fallback) {
			if (TryRead (key, out char value)) {
				return value;
			}
			return fallback;
		}

		/// <summary>
		/// Reads an <see cref="sbyte"/> value with the given key and <see cref="Encoding"/>.<br></br>
		/// If no encoding is given, <see cref="Encoding.UTF8"/> will be used.<br></br>
		/// Returns <paramref name="fallback"/>, if the key was not found or the associated value was empty.
		/// </summary>
		public string Read (string key, string fallback, Encoding encoding = null) {
			if (TryRead (key, out string value, encoding)) {
				return value;
			}
			return fallback;
		}

		/// <summary>
		/// Reads a <see cref="byte"/>[] value with the given key.<br></br>
		/// Returns <paramref name="fallback"/>, if the key was not found or the associated value was not a <see cref="byte"/>[].
		/// </summary>
		public byte[] Read (string key, byte[] fallback) {
			if (TryRead (key, out byte[] value)) {
				return value;
			}
			return fallback;
		}

	}
}