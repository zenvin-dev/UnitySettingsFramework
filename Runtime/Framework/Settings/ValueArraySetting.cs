using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zenvin.Settings.Framework {
	/// <summary>
	/// Non-generic base class for a Setting that contains a fixed array of values.<br></br>
	/// The Setting's actual value represents the index used to retrieve values from that array.
	/// </summary>
	public abstract class ValueArraySetting : SettingBase<int>, IEnumerable<object> {
		private protected object[] values;


		/// <summary>
		/// The number of values managed by this Setting.
		/// </summary>
		public int Length => values?.Length ?? 0;

		/// <summary>
		/// Returns one of the Setting's values at a given index.
		/// </summary>
		public object GetValue (int index) {
			if (index < 0 || index >= Length) {
				throw new IndexOutOfRangeException (nameof (index));
			}
			return values[index];
		}

		/// <summary>
		/// Called during initialization. Should contain the list of values managed by this Setting.
		/// </summary>
		protected abstract object[] GetValueArray ();

		/// <summary>
		/// Stand-in for the inherited <see cref="SettingBase{T}.OnInitialize"/>, which is sealed in <see cref="ValueArraySetting"/>.<br></br>
		/// Will be called right after the Setting's values have been initialized.
		/// </summary>
		protected virtual void OnPostInitialize () { }


		protected sealed override void OnInitialize () {
			values = GetValueArray () ?? Array.Empty<object>();
			OnPostInitialize ();
		}

		protected override void ProcessValue (ref int value) {
			if (value >= values.Length) {
				value = values.Length - 1;
			}
			if (value < 0) {
				value = 0;
			}
		}


		public IEnumerator<object> GetEnumerator () {
			return ((IEnumerable<object>)values).GetEnumerator ();
		}
		
		IEnumerator IEnumerable.GetEnumerator () {
			return values.GetEnumerator ();
		}
	}

	public abstract class ValueArraySetting<T> : ValueArraySetting, IEnumerable<T> {

		[SerializeField] private T typedDefaultValue;


		public T CachedValueTyped => values.Length == 0 ? default : (T)values[CachedValue];
		public T CurrentValueTyped => values.Length == 0 ? default : (T)values[CurrentValue];

		public T this[int index] => index < 0 || index >= Length ? throw new IndexOutOfRangeException (nameof (index)) : (T)values[index];


		protected override int OnSetupInitialDefaultValue () {
			return Array.IndexOf (values, typedDefaultValue);
		}


		IEnumerator<T> IEnumerable<T>.GetEnumerator () {
			for (int i = 0; i < Length; i++) {
				yield return this[i];
			}
		}
	}
}