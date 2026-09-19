using UnityEngine;

namespace UI.Settings {
	[CreateAssetMenu(menuName = "Scriptable Objects/Settings Page")]
	class SettingsPage : ScriptableObject {
		public string               label;
		public SettingsDefinition[] definition;
	}
}