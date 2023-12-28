using Zenvin.Settings.Framework;
using UnityEngine;

namespace Zenvin.Settings.UI {
	/// <summary>
	/// An implementation of <see cref="SettingControl{TControlType, THandledType}"/>
	/// that allows for manually assigning a <see cref="SettingBase{T}"/> (<typeparamref name="TControlType"/>) through the editor.
	/// </summary>
	/// <typeparam name="TControlType"> The most basic type of <see cref="SettingBase{T}"/> that this control can handle. </typeparam>
	/// <typeparam name="THandledType"> The type used by the handled <see cref="SettingBase{T}"/>. </typeparam>
	public abstract class AssignedSettingControl<TControlType, THandledType> : SettingControl<TControlType, THandledType> where TControlType : SettingBase<THandledType> {

		[Tooltip("The value of this field will be used to set up the control when it first becomes active.")]
		[SerializeField] private TControlType assignedSetting;
		[Tooltip("If enabled, the control's GameObject will be disabled, if there is no assigned setting.")]
		[SerializeField] private bool disableWhenMissingSetting = true;


		private void Start () {
			if (SettingRaw != null) {
				return;
			}
			if (assignedSetting == null) {
				if (disableWhenMissingSetting) {
					gameObject.SetActive (false);
				}
				return;
			}

			Setup (assignedSetting);
		}
	}
}