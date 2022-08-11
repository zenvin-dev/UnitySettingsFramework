using System;

namespace Zenvin.Settings.Framework.Serialization {
	public partial class ValuePacket {

		public bool TryReadSByte (string key, out sbyte value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (sbyte)) {
				value = (sbyte)raw[0];
				return true;
			}
			value = default;
			return false;
		}

		public bool TryReadByte (string key, out byte value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (byte)) {
				value = raw[0];
				return true;
			}
			value = default;
			return false;
		}

		public bool TryReadInt16 (string key, out short value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (short)) {
				value = BitConverter.ToInt16 (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		public bool TryReadUInt16 (string key, out ushort value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (ushort)) {
				value = BitConverter.ToUInt16 (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		public bool TryReadInt32 (string key, out int value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (int)) {
				value = BitConverter.ToInt32 (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		public bool TryReadUInt32 (string key, out uint value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (uint)) {
				value = BitConverter.ToUInt32 (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		public bool TryReadInt64 (string key, out long value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (long)) {
				value = BitConverter.ToInt64 (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		public bool TryReadUInt64 (string key, out ulong value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (ulong)) {
				value = BitConverter.ToUInt64 (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		public bool TryReadSingle (string key, out float value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (float)) {
				value = BitConverter.ToSingle (raw, 0);
				return true;
			}
			value = default;
			return false;
		}

		public bool TryReadDouble (string key, out double value) {
			if (TryRead (key, out byte[] raw) && raw != null && raw.Length == sizeof (double)) {
				value = BitConverter.ToDouble (raw, 0);
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