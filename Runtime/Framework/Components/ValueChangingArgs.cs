using UnityEngine;
using static Zenvin.Settings.Framework.SettingBase.ValueChangeMode;

namespace Zenvin.Settings.Framework.Components {
	/// <summary>
	/// Event arguments containing information about an impending Setting value change.
	/// </summary>
	public class ValueChangingArgs<TValue> {
		/// <summary> The Setting whose value is changing. </summary>
		public readonly SettingBase Setting;
		/// <summary> The mode in which the Setting's value is changing. </summary>
		public readonly SettingBase.ValueChangeMode Mode;
		/// <summary> The value that the Setting would have after the change. </summary>
		public readonly TValue NewValue;

		/// <summary> 
		/// Whether it was requested that the change not take place. <br></br>
		/// See <see cref="PreventChange"/>.
		/// </summary>
		public bool ChangePrevented { get; private set; }
		/// <summary>
		/// Whether the Setting change can actually be prevented. <br></br>
		/// See <see cref="PreventChange"/>.
		/// </summary>
		public bool CanPreventChange => Mode != Initialize && Mode != Deserialize && Mode != Notify && Mode != Prevent;


		private ValueChangingArgs () { }

		internal ValueChangingArgs (SettingBase setting, SettingBase.ValueChangeMode mode, TValue newValue) {
			Setting = setting;
			Mode = mode;
			NewValue = newValue;
		}


		/// <summary>
		/// Requests the change to be prevented. <br></br>
		/// This will <b>not have an effect</b> during <c>Initialize</c>, <c>Deserialize</c> or <c>Notify</c> operations!
		/// </summary>
		public void PreventChange () {
			if (Setting == null) {
				return;
			}
			if (!CanPreventChange) {
				Debug.LogError ($"Cannot prevent Setting value change with Mode {Mode}.");
				return;
			}

			ChangePrevented = true;
		}
	}
}
