using static Zenvin.Settings.Loading.RuntimeSettingLoader;
using GroupFacDict = System.Collections.Generic.Dictionary<string, Zenvin.Settings.Loading.IGroupFactory>;
using SettingFacDict = System.Collections.Generic.Dictionary<string, Zenvin.Settings.Loading.ISettingFactory>;

namespace Zenvin.Settings.Loading {
	internal static class RuntimeLoaderHelpers {
		public static void PopulateFactoryDicts (TypeFactoryWrapper[] factories, SettingFacDict settingFactories, GroupFacDict groupFactories) {
			if (factories == null || factories.Length == 0 || settingFactories == null || groupFactories == null) {
				return;
			}

			foreach (var f in factories) {
				string fType = f.Type;

				switch (f.FactoryType) {
					case TypeFactoryWrapper.FactoryResult.Group:
						PopulateGroupFactoryDict (f.GroupFactory, groupFactories, fType);
						break;
					case TypeFactoryWrapper.FactoryResult.Setting:
						PopulateSettingFactoryDict (f.SettingFactory, settingFactories, fType);
						break;
				}
			}
		}

		private static void PopulateGroupFactoryDict (IGroupFactory factory, GroupFacDict groupFactories, string fType) {
			if (factory == null)
				return;

			if (string.IsNullOrEmpty (fType))
				fType = factory.GetDefaultValidType ();

			if (!string.IsNullOrEmpty (fType))
				groupFactories[fType] = factory;
		}

		private static void PopulateSettingFactoryDict (ISettingFactory factory, SettingFacDict settingsFactories, string fType) {
			if (factory == null)
				return;

			if (string.IsNullOrEmpty (fType))
				fType = factory.GetDefaultValidType ();

			if (!string.IsNullOrEmpty (fType))
				settingsFactories[fType] = factory;
		}
	}
}
