using UnityEngine;

namespace UI.Settings {
	abstract class SettingsDefinition : ScriptableObject {
		public string id;
		public string label;
		public string description;
	}
}