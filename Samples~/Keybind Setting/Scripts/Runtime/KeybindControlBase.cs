using TMPro;
using UnityEngine;
using Zenvin.Settings.UI;
using RebindOp = UnityEngine.InputSystem.InputActionRebindingExtensions.RebindingOperation;

namespace Zenvin.Settings.Samples {
	public abstract class KeybindControlBase : AssignedSettingControl<KeybindSetting, Keybinding> {

		private static RebindOp operation;

		[SerializeField] private RebindOperationSettings rebindSettings;


		/// <summary>
		/// Calls <see cref="Framework.SettingBase.ResetValue(bool)"/> on the control's associated Setting. <br></br> 
		/// Meant for resetting individual key binds through UI.
		/// </summary>
		public void ResetSetting () {
			Setting.ResetValue (false);
		}

		/// <summary>
		/// Attempts to start rebinding the input binding assigned to the referenced <see cref="SettingControl{TControlType, THandledType}.Setting"/>.
		/// </summary>
		public void StartRebind () {
			// Cancel any currently running rebind operation
			Cleanup ();

			// Cancel if there is no Setting assigned to the Control
			if (Setting == null) {
				return;
			}

			// Get active rebind settings
			var rebindOpSettings = Setting.RebindSettings;
			// If there are no rebind settings set in the Setting object
			if (rebindOpSettings == null) {
				// Use the rebind settings referenced in the Control
				rebindOpSettings = rebindSettings;
			}
			// Cancel if no settings for rebinding are given
			if (rebindOpSettings == null) {
				Debug.LogError ($"Cannot rebind {Setting}, because RebindSettings are missing.");
				return;
			}

			// Create a new RebindingOperation from the rebindSettings
			operation = rebindOpSettings.GetOperation (Setting.ExcludeControls, Setting.GetActiveBindingMask());
			// Cancel if creating the operation was not successful
			if (operation == null) {
				return;
			}

			// Set up the operation's callback handlers
			operation
				.OnApplyBinding(ApplyRebindingOperationHandler)
				.OnComplete (CompletedRebindingOperationHandler)
				.OnCancel (CancelledRebindingOperationHandler);

			// Start the operation to listen for input to rebind with
			operation.Start ();
			// Notify the control that rebinding started
			OnStartedRebind ();
		}


		/// <summary>
		/// Called when <see cref="StartRebind"/> was called successfully.
		/// </summary>
		protected abstract void OnStartedRebind ();

		/// <summary>
		/// Called after the operation started by <see cref="StartRebind"/> has ended.
		/// </summary>
		/// <param name="cancelled"> Whether the operation that triggered the method call was cancelled. </param>
		protected abstract void OnEndedRebind (bool cancelled);


		private void CancelledRebindingOperationHandler (RebindOp operation) {
			OnEndedRebind (true);
			Cleanup ();
		}

		private void CompletedRebindingOperationHandler (RebindOp operation) {
			OnEndedRebind (false);
			Cleanup ();
		}

		private void ApplyRebindingOperationHandler (RebindOp operation, string path) {
			Setting.SetValue (path);
		}

		private void Cleanup () {
			if (operation == null)
				return;

			if (operation.started && !operation.completed && !operation.canceled)
				operation.Cancel ();

			operation.Dispose ();
			operation = null;
		}
	}
}
