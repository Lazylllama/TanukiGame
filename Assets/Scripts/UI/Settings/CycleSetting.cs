using UnityEngine;

namespace UI.Settings {
	[CreateAssetMenu(menuName = "Scriptable Objects/Cycle Setting")]
	class CycleSettings : SettingsDefinition {
		public string[] options;
		public int      defaultIndex;
	}
}