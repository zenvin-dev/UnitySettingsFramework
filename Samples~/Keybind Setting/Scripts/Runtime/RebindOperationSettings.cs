using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using RebindOp = UnityEngine.InputSystem.InputActionRebindingExtensions.RebindingOperation;

namespace Zenvin.Settings.Samples {
	[CreateAssetMenu (menuName = "Scriptable Objects/Zenvin/Rebind Operation Settings", order = 201)]
	public sealed class RebindOperationSettings : ScriptableObject {

		public enum CancelControlSource {
			Actions,
			Bindings,
			Both,
		}

		[Header("Cancellation")]
		[SerializeField] private CancelControlSource rebindCancelSource = CancelControlSource.Both;
		[SerializeField] private InputActionReference[] cancelActionReferences;
		[SerializeField, InputControl] private List<string> cancelActionBindings;

		[Header("Binding Masking & Exclusion")]
		[SerializeField, InputControl] private List<string> excludeBindings;
		[SerializeField] private bool useMask = false;
		[SerializeField] private InputBinding bindingMask;

		[Header("Filtering")]
		[SerializeField] private bool suppressMatchingEvents = true;
		[SerializeField] private bool ignoreNoisyControls = true;
		[SerializeField, Min(0f)] private float minMagnitude = 0.5f;

		[Header("Behaviour")]
		[SerializeField] private bool hasTimeout = true;
		[SerializeField] private float timeout = 10f;
		[SerializeField] private bool generalizeControlPath = true;


		public InputBinding? BindingMask => useMask ? bindingMask : (InputBinding?)null;


		public RebindOp GetOperation (string[] excludeBindings, InputBinding? mask) {
			var operation = new RebindOp ()
				.WithBindingMask (mask);

			operation = SetupCancellation (operation);
			operation = SetupTimeout (operation);
			operation = SetupExclusion (operation, this.excludeBindings);
			operation = SetupExclusion (operation, excludeBindings);
			operation = SetupFiltering (operation);

			if(!generalizeControlPath)
				operation = operation.WithoutGeneralizingPathOfSelectedControl ();

			return operation;
		}

		private RebindOp SetupCancellation (RebindOp operation) {
			var useCtrl = rebindCancelSource == CancelControlSource.Actions || rebindCancelSource == CancelControlSource.Both;
			var useBind = rebindCancelSource == CancelControlSource.Bindings || rebindCancelSource == CancelControlSource.Both;

			if (cancelActionReferences != null && cancelActionReferences.Length > 0 && useCtrl) {
				foreach (var action in cancelActionReferences) {
					if (action != null && action.action != null) {
						foreach (var control in action.action.controls) {
							operation = operation.WithCancelingThrough (control);
						}
					}
				}
			}

			if (cancelActionBindings != null && cancelActionBindings.Count > 0 && useBind) {
				foreach (var binding in cancelActionBindings) {
					if (!string.IsNullOrWhiteSpace (binding)) {
						operation = operation.WithCancelingThrough (binding);
					}
				}
			}

			return operation;
		}

		private RebindOp SetupTimeout (RebindOp operation) {
			if (hasTimeout) {
				operation = operation.WithTimeout (timeout);
			}
			return operation;
		}

		private RebindOp SetupExclusion (RebindOp operation, ICollection<string> bindings) {
			if (bindings == null || bindings.Count == 0)
				return operation;

			foreach (var binding in bindings) {
				if (string.IsNullOrWhiteSpace (binding))
					continue;

				operation = operation.WithControlsExcluding (binding);
			}

			return operation;
		}

		private RebindOp SetupFiltering (RebindOp operation) {
			operation = operation
				.WithMatchingEventsBeingSuppressed (suppressMatchingEvents)
				.WithMagnitudeHavingToBeGreaterThan(minMagnitude);

			if(!ignoreNoisyControls)
				operation = operation.WithoutIgnoringNoisyControls ();

			return operation;
		}


#if UNITY_EDITOR
		[ContextMenu ("Add default Cancel Bindings")]
		private void AddDefaultCancelBindings () {
			cancelActionBindings ??= new List<string> ();

			AddIfNotPresent (cancelActionBindings, "<Keyboard>/escape");
			AddIfNotPresent (cancelActionBindings, "<Any>/cancel");
		}

		[ContextMenu ("Add default Excluded Bindings")]
		private void AddDefaultExcludedBindings () {
			excludeBindings ??= new List<string> ();

			AddIfNotPresent (excludeBindings, "<Pointer>/position");
			AddIfNotPresent (excludeBindings, "<Pointer>/delta");
			AddIfNotPresent (excludeBindings, "<Mouse>/clickCount");
			AddIfNotPresent (excludeBindings, "<VirtualMouse>/clickCount");
		}

		private static void AddIfNotPresent<T> (List<T> list, T item) {
			if (!list.Contains (item))
				list.Add (item);
		}
#endif
	}
}
