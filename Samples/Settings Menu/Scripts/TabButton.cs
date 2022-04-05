using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Zenvin.Settings.Samples {
	public class TabButton : MonoBehaviour {

		private TabView view;
		private int index;

		[SerializeField] private Image image;
		[SerializeField] private TextMeshProUGUI text;


		public void Setup (TabView view, int index, string label) {
			this.view = view;
			this.index = index;

			text?.SetText (label);
		}

		public void OnClick () {
			view.SelectTab (index);
		}

		public void OnTabStateChanged (bool active) {
			image.enabled = active;
		}
		
	}
}