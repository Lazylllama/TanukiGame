using System;
using UnityEngine;

namespace UI.Settings {
	[CreateAssetMenu(menuName = "Scriptable Objects/Slider Setting")]
	class PointerSetting : SettingsDefinition {
		public SettingsPage pointer;

		public override string GetDisplayValue() {
			return "";
		}

		public override void Step(bool forward = true, int set = -1) {
			
		}
	}
}