using Newtonsoft.Json.Linq;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using Zenvin.Settings.Framework;
using Zenvin.Settings.Framework.Serialization;
using InputBindDisplayOpts = UnityEngine.InputSystem.InputBinding.DisplayStringOptions;

namespace Zenvin.Settings.Samples {
	public sealed class KeybindSetting : SettingBase<Keybinding>, ISerializable<JObject>, ISerializable<ValuePacket> {
		public enum InputMaskUsage {
			None,
			FromRebindSettings,
			Local,
		}

		private const string BindingIdKey = "Id";
		private const string BindingPathKey = "Path";

		[SerializeField] private bool hideIfInvalid = true;
		[SerializeField] private RebindOperationSettings rebindSettings;
		[SerializeField] private InputActionBinding binding = new InputActionBinding ();
		[SerializeField] private bool allowAnonymousBindings = true;
		[SerializeField] private bool applyOnSet = true;
		[Space]
		[SerializeField, InputControl] private string[] excludeControls;
		[SerializeField] private InputMaskUsage useMask = InputMaskUsage.FromRebindSettings;
		[SerializeField] private InputBinding bindingMask;
		[SerializeField] private bool enableDebugLogs = false;

		public RebindOperationSettings RebindSettings => rebindSettings;
		public string BindingId => binding.BindingId;
		public string[] ExcludeControls => excludeControls;


		public bool GetIsRebindSetupValid () => binding.IsValid (out _);

		public void SetValue (string path) {
			var value = DefaultValue;
			value.Path = path;
			SetValue (value);
		}

		public string GetDisplayTextForCurrentValue (InputBindDisplayOpts displayOptions = InputBindDisplayOpts.DontIncludeInteractions, string errorFallback = "N/A") {
			if (!binding.IsValid (out var bindingId))
				return errorFallback;

			var action = binding.ActionReference.action;
			if (!action.TryGetBindingIndex (bindingId, out var index))
				return errorFallback;

			return action.GetBindingDisplayString (index, displayOptions);
		}

		public InputBinding? GetActiveBindingMask () {
			return useMask switch {
				InputMaskUsage.FromRebindSettings => rebindSettings == null ? bindingMask : rebindSettings.BindingMask,
				InputMaskUsage.Local => bindingMask,
				_ => null,
			};
		}


		/// <inheritdoc/>
		protected override void OnInitialize () {
			if (hideIfInvalid && !GetIsRebindSetupValid ()) {
				SetVisibilityWithoutNotify (SettingVisibility.Hidden);
			}
		}

		/// <inheritdoc/>
		protected override Keybinding OnSetupInitialDefaultValue () {
			// Extract the target binding from the binding set in the inspector to set up the default Keybinding value
			_ = KeybindUtility.TryGetKeybinding (binding, ActionPath.Original, out var defaultBinding);
			return defaultBinding;
		}

		/// <inheritdoc/>
		protected override void OnValueChanged (ValueChangeMode mode) {
			if (!binding.IsValid (out _))
				return;

			switch (mode) {
				case ValueChangeMode.Apply:
					ApplyOverride (CurrentValue.Path);
					break;
				case ValueChangeMode.Set:
					if (applyOnSet) {
						ApplyOverride (CachedValue.Path);
					}
					break;
			}
		}

		/// <inheritdoc/>
		protected override bool CompareEquality (Keybinding a, Keybinding b) {
			// The Keybindings should be considered equal if they point to the same path, or are both empty
			return (string.IsNullOrWhiteSpace (a.Path) && string.IsNullOrWhiteSpace (b.Path))
				   || a.Path == b.Path;
		}

		/// <inheritdoc/>
		protected override void ProcessValue (ref Keybinding value) {
			// If no Id is contained in the given value, assume it's okay to be set
			if (allowAnonymousBindings && string.IsNullOrWhiteSpace (value.Id)) {
				Log ("Passing anonymous keybind change.");
				return;
			}

			// Prevent changes if Id of new value does not match that of the target binding
			if (value.Id.Equals (BindingId, System.StringComparison.OrdinalIgnoreCase)) {
				return;
			}

			value = CurrentValue;
			Log ($"Prevented keybind change because of invalid Id or Id mismatch (Target Binding: '{BindingId}', New Binding: '{value.Id}')");
		}


		private void ApplyOverride (string path) {
			if (!binding.IsValid (out var bindingId)) {
				Log ("Could not apply override because Target Binding was invalid.");
				return;
			}

			var action = binding.ActionReference.action;
			if (!action.TryGetBindingIndex (bindingId, out var index)) {
				Log ($"Could not apply override because binding index was not found (Id '{bindingId}')");
				return;
			}

			var actionBinding = action.bindings[index];
			actionBinding.overridePath = path;
			action.ApplyBindingOverride (index, actionBinding);
			Log ($"Applied binding '{path}' to Target Binding '{bindingId}'.");
		}

		private void Log (string message) {
			if (enableDebugLogs) {
				Debug.Log ($"[{ToString ()}] {message}");
			}
		}


		void ISerializable<ValuePacket>.OnSerialize (ValuePacket value) {
			if (!binding.IsValid (out _))
				return;

			value.Write (BindingIdKey, CurrentValue.Id);
			value.Write (BindingPathKey, CurrentValue.Path);
		}

		void ISerializable<ValuePacket>.OnDeserialize (ValuePacket value) {
			if (!binding.IsValid (out var bindingId))
				return;

			if (!value.TryRead (BindingIdKey, out string id) || !value.TryRead (BindingPathKey, out string path))
				return;

			if (string.IsNullOrWhiteSpace (id) || !bindingId.ToString ().Equals (id, System.StringComparison.OrdinalIgnoreCase))
				return;

			SetValue (path);
		}

		void ISerializable<JObject>.OnSerialize (JObject value) {
			if (!binding.IsValid (out _))
				return;

			value.Add (BindingIdKey, CurrentValue.Id);
			value.Add (BindingPathKey, CurrentValue.Path);
		}

		void ISerializable<JObject>.OnDeserialize (JObject value) {
			if (!binding.IsValid (out var bindingId))
				return;

			if (!value.TryGetValue (BindingIdKey, out var idToken) || !value.TryGetValue (BindingPathKey, out var pathToken))
				return;

			if (idToken.Type != JTokenType.String || pathToken.Type != JTokenType.String)
				return;

			var id = (string)idToken;
			var path = (string)pathToken;
			if (string.IsNullOrWhiteSpace (id) || !BindingId.ToString ().Equals (id, System.StringComparison.OrdinalIgnoreCase))
				return;

			SetValue (path);
		}
	}
}
