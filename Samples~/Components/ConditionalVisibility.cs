using System;
using UnityEngine;
using Zenvin.Settings.Framework;
using Zenvin.Settings.Framework.Components;

namespace Zenvin.Settings.Samples {
	public class ConditionalVisibility : FrameworkComponent {
		public enum TargetValue {
			CachedValue,
			CurrentValue,
		}

		[SerializeField] private SettingReference<bool> condition;
		[SerializeField] private TargetValue targetValue = TargetValue.CurrentValue;
		[SerializeField] private SettingVisibility enabledVisibility = SettingVisibility.Visible;
		[SerializeField] private SettingVisibility disabledVisibility = SettingVisibility.Disabled;


		public override void OnInitialize () {
			if (condition == null)
				return;

			condition.ValueChanged += ConditionValueChangedHandler;
			UpdateVisibility ();
		}

		public override void OnCreateWithValues (StringValuePair[] values) {
			if (values == null)
				return;

			for (int i = 0; i < values.Length; i++) {
				var value = values[i];
				switch (value.Key) {
					case "condition":
						UpdateConditionFromGuid (value.Value);
						break;
					case "targetValue":
						Enum.TryParse (value.Value, true, out targetValue);
						break;
					case "enabledVisibility":
						Enum.TryParse (value.Value, true, out enabledVisibility);
						break;
					case "disabledVisibility":
						Enum.TryParse (value.Value, true, out disabledVisibility);
						break;
				}
			}
		}


		private void ConditionValueChangedHandler (SettingBase.ValueChangeMode mode) {
			UpdateVisibility ();
		}

		private void UpdateVisibility () {
			var visibility = GetGoalVisibility ();
			BaseContainer.SetVisibility (visibility);
		}

		private SettingVisibility GetGoalVisibility () {
			if (condition == null)
				return BaseContainer.Visibility;

			var state = targetValue switch {
				TargetValue.CachedValue => condition.CachedValue,
				TargetValue.CurrentValue => condition.CurrentValue,
				_ => true
			};
			return state ? enabledVisibility : disabledVisibility;
		}


		private void UpdateConditionFromGuid (string guid) {
			if (condition != null || string.IsNullOrWhiteSpace (guid))
				return;

			var asset = BaseContainer switch {
				SettingBase setting => setting.Asset,
				SettingsGroup group => GetRootAsset (group),
				_ => null
			};
			if (asset == null)
				return;

			condition = asset.GetSettingReference<bool> (guid);
		}

		private SettingsAsset GetRootAsset (SettingsGroup group) {
			if (group == null)
				return null;

			var asset = group as SettingsAsset;
			while (asset == null && group != null) {
				group = group.Parent;
				asset = group as SettingsAsset;
			}
			return asset;
		}
	}
}
