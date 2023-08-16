using System.Collections.Generic;
using static Zenvin.Settings.Loading.RuntimeSettingLoader;
using IGF = Zenvin.Settings.Loading.IGroupFactory;
using ISF = Zenvin.Settings.Loading.ISettingFactory;

namespace Zenvin.Settings.Loading {
	internal static class RuntimeLoaderHelpers {
		public static void PopulateFactoryDicts (TypeFactoryWrapper[] factories, Dictionary<string, ISF> settingFactories, Dictionary<string, IGF> groupFactories) {
			if (factories == null || factories.Length == 0 || settingFactories == null || groupFactories == null) {
				return;
			}

			foreach (var f in factories) {
				string fType = f.Type;

				if (f.IsGroupFactory) {
					PopulateGroupFactoryDict (f.GroupFactory, groupFactories, fType);
				} else {
					PopulateSettingFactoryDict (f.SettingFactory, settingFactories, fType);
				}
			}
		}

		private static void PopulateGroupFactoryDict (IGF factory, Dictionary<string, IGF> groupFactories, string fType) {
			if (factory != null) {
				if (string.IsNullOrEmpty (fType)) {
					fType = factory.GetDefaultValidType ();
				}

				if (!string.IsNullOrEmpty (fType)) {
					groupFactories[fType] = factory;
				}
			}
		}

		private static void PopulateSettingFactoryDict (ISF factory, Dictionary<string, ISF> settingsFactories, string fType) {
			if (factory != null) {
				if (string.IsNullOrEmpty (fType)) {
					fType = factory.GetDefaultValidType ();
				}

				if (!string.IsNullOrEmpty (fType)) {
					settingsFactories[fType] = factory;
				}
			}
		}
	}
}