using System;
using UnityEngine.InputSystem;

namespace Zenvin.Settings.Samples {
	public enum ActionPath {
		Original,
		Override,
		Effective,
	}
		
	public static class KeybindUtility {
		public static bool TryGetKeybinding (InputActionBinding inputAction, ActionPath path, out Keybinding binding) {
			binding = default;
			if (!inputAction.IsValid (out var bindingId))
				return false;

			var action = inputAction.ActionReference.action;
			if (!action.TryGetBindingById (bindingId, out var actionBinding))
				return false;

			var actionPath = path switch {
				ActionPath.Original => actionBinding.path,
				ActionPath.Override => actionBinding.overridePath,
				ActionPath.Effective => actionBinding.effectivePath,
				_ => null
			};
			if (actionPath == null)
				return false;

			binding = new Keybinding {
				Id = bindingId.ToString(),
				Path = actionPath,
			};
			return true;
		}

		public static bool IsValid (this InputActionBinding binding) {
			return IsValid (binding, out _);
		}

		public static bool IsValid (this InputActionBinding binding, out Guid bindingId) {
			if (binding == null)
				return false;
			if (binding.ActionReference == null)
				return false;
			if (binding.ActionReference.action == null)
				return false;
			if (string.IsNullOrWhiteSpace (binding.BindingId))
				return false;

			if (!Guid.TryParse (binding.BindingId, out bindingId))
				return false;

			return true;
		}

		public static bool TryGetBindingById (this InputAction action, Guid bindingId, out InputBinding binding) {
			binding = default;
			if (action == null)
				return false;

			foreach (var actionBinding in action.bindings) {
				if (actionBinding.id == bindingId) {
					binding = actionBinding;
					return true;
				}
			}

			return false;
		}

		public static bool TryGetBindingIndex (this InputAction action, string bindingId, out int index) {
			index = -1;
			return Guid.TryParse (bindingId, out var guid) && TryGetBindingIndex (action, guid, out index);
		}

		public static bool TryGetBindingIndex (this InputAction action, Guid bindingId, out int index) {
			index = -1;
			if (action == null)
				return false;

			var bindings = action.bindings;
			for (int i = 0; i < bindings.Count; i++) {
				if (bindings[i].id == bindingId) {
					index = i;
					return true;
				}
			}

			return false;
		}
	}
}
