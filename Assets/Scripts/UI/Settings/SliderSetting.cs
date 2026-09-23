using UnityEngine;

namespace UI.Settings {
	[CreateAssetMenu(menuName = "Scriptable Objects/Slider Setting")]
	class SliderSettings : SettingsDefinition {
		public int defaultValue, maxValue;
		private int Value {get => SettingService.GetInt(id, defaultValue); set => SettingService.SetInt(id, value); }

		public override string GetDisplayValue() => $"{Value}/{maxValue}";

		public override void Step(bool forward = true) {
			Value = Mathf.Clamp((Value + (forward ? 1 : -1) ), 0, maxValue);
		}
	}
}