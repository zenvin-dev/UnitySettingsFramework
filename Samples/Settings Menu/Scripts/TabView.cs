using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using Zenvin.Settings.Framework;

namespace Zenvin.Settings.Samples {
	[DisallowMultipleComponent]
	public class TabView : MonoBehaviour {

		private readonly List<TabData> tabs = new List<TabData> ();
		private int activeTab = 0;

		[SerializeField] private LayoutGroup tabButtonParent;
		[SerializeField] private TabButton tabButtonPrefab;
		[Space, SerializeField] private RectTransform tabContentParent;
		[SerializeField] private RectTransform tabContentPrefab;


		public RectTransform AddTab (SettingsGroup group) {
			if (group == null) {
				return null;
			}

			TabButton button = Instantiate (tabButtonPrefab);
			button.Setup (this, tabs.Count, group.Name);
			button.transform.SetParent (tabButtonParent.transform);
			button.transform.localScale = Vector3.one;

			RectTransform content = Instantiate (tabContentPrefab);
			content.SetParent (tabContentParent);
			content.localScale = Vector3.one;

			tabs.Add (new TabData () { Button = button, ContentParent = content, Group = group });

			SelectTab (0);
			return content;
		}

		public void SelectTab (int index) {
			activeTab = index;
			for (int i = 0; i < tabs.Count; i++) {
				tabs[i].Button.OnTabStateChanged (i == index);
				tabs[i].ContentParent.gameObject.SetActive (i == index);
			}
		}


		public class TabData {
			public TabButton Button;
			public SettingsGroup Group;
			public RectTransform ContentParent;
		}

	}
}