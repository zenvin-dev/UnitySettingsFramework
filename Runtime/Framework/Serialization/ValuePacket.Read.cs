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

	}
}