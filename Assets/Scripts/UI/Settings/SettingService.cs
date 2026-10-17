using System;
using Logic;
using UnityEngine;

namespace UI.Settings {
	public static class SettingService {
		public static event Action<string> OnSettingChanged;

		//? Integer
		public static void SetInt(string id, int value) {
			var settings = SaveSystem.Data.intSettings;
			var index    = settings.FindIndex(s => s.id == id);

			switch (index) {
				case >= 0 when settings[index].value == value:
					return; //? already set
				case >= 0:
					settings[index] = new IntSetting { id = id, value = value };
					break;
				default:
					settings.Add(new IntSetting { id = id, value = value });
					break;
			}

			SaveSystem.Save();
			OnSettingChanged?.Invoke(id);
		}

		public static int GetInt(string id, int fallback) {
			var index = SaveSystem.Data.intSettings.FindIndex(s => s.id == id);
			return index >= 0 ? SaveSystem.Data.intSettings[index].value : fallback;
		}
	}
}