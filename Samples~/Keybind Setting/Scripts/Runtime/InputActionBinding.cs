using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Zenvin.Settings.Samples {
    [Serializable]
	public sealed class InputActionBinding {
        [SerializeField] private InputActionReference actionReference;
        [SerializeField] private string bindingId;

        public InputActionReference ActionReference => actionReference;
        public string BindingId => bindingId;
    }
}
