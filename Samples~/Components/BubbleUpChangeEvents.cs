using UnityEngine;
using Zenvin.Settings.Framework;
using Zenvin.Settings.Framework.Components;

namespace Zenvin.Settings.Samples {
	public class BubbleUpChangeEvents : TypedFrameworkComponent<SettingsGroup>, ISettingEventReceiver {
		[SerializeField] private bool bubbleUp = true;
		[SerializeField, Min (0)] private int skipLevels = 0;


		void ISettingEventReceiver.OnValueChanging (SettingBase setting, SettingBase.ValueChangeMode mode) {
			var target = GetTargetGroup ();
			if (target != null && target.TryGetComponent (out ISettingEventReceiver receiver)) {
				receiver.OnValueChanged (setting, mode);
			}
		}

		void ISettingEventReceiver.OnValueChanged (SettingBase setting, SettingBase.ValueChangeMode mode) {
			var target = GetTargetGroup ();
			if (target != null && target.TryGetComponent (out ISettingEventReceiver receiver)) {
				receiver.OnValueChanged (setting, mode);
			}
		}


		private SettingsGroup GetTargetGroup () {
			var skipped = 0;
			var target = Container;
			while (target != null && skipped < skipLevels) {
				target = target.Parent;
				skipped++;
			}

			return target;
		}
	}
}
