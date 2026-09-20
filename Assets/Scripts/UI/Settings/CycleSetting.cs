using UnityEngine;

namespace UI.Settings {
	[CreateAssetMenu(menuName = "Scriptable Objects/Cycle Setting")]
	class CycleSettings : SettingsDefinition {
		public  string[] options;
		public  int      defaultIndex;
		public int      Value {get => SettingService.GetInt(id, defaultIndex); set => SettingService.SetInt(id, value); }

		public override string GetDisplayValue() => options[Value];

		public override void Step() {
			Value = (Value + 1) % options.Length;
		}
	}
}