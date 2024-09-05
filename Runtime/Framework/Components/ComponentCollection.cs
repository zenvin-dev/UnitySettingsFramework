using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zenvin.Settings.Framework.Components {
	[Serializable]
	public class ComponentCollection {
		[SerializeField] private List<FrameworkComponent> components;


		public int Count => components?.Count ?? 0;
		internal FrameworkComponent this[int index] => components == null || index < 0 || index >= Count ? null : components[index];


		internal bool Add (ComposedFrameworkObject container, FrameworkComponent component, bool checkContainer) {
			if (component == null || container == null)
				return false;
			if (checkContainer && component.BaseContainer != null)
				return component.BaseContainer == container;
			if (components != null && components.Contains (component))
				return true;
			if (!component.IsValidContainer (container))
				return false;

			components ??= new List<FrameworkComponent> ();
			components.Add (component);
			component.BaseContainer = container;
			return true;
		}

		internal bool Remove (ComposedFrameworkObject container, FrameworkComponent component) {
			if (component == null || container == null)
				return false;
			if (components == null || components.Count == 0)
				return false;
			if (component.BaseContainer != null && component.BaseContainer != container)
				return false;
			if (!components.Remove (component))
				return false;

			component.BaseContainer = container;
			return true;
		}

		internal void OnValueChanging (SettingBase setting, SettingBase.ValueChangeMode mode) {
			if (components == null || components.Count == 0 || setting == null)
				return;

			for (int i = 0; i < components.Count; i++) {
				if (components[i] is ISettingEventReceiver receiver) {
					receiver.OnValueChanging (setting, mode);
				}
			}
		}

		internal bool OnValueChanging<T> (ValueChangingArgs<T> args) {
			if (components == null || components.Count == 0)
				return false;

			var receiverPresent = false;
			for (int i = 0; i < components.Count; i++) {
				if (components[i] is ISettingEventReceiver<T> receiver) {
					receiver.OnValueChanging (args);
					receiverPresent = true;
				}
			}
			return receiverPresent;
		}

		internal void OnValueChanged (SettingBase setting, SettingBase.ValueChangeMode mode) {
			if (components == null || components.Count == 0 || setting == null)
				return;

			for (int i = 0; i < components.Count; i++) {
				if (components[i] is ISettingEventReceiver receiver) {
					receiver.OnValueChanged (setting, mode);
				}
			}
		}
	}
}
