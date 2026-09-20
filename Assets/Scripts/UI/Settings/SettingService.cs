using System;
using UnityEngine;

namespace UI.Settings {
	public static class SettingService {
		public static event Action<string> OnSettingChanged;

		public static void SetInt(string id, int value) {
			PlayerPrefs.SetInt(id, value);
			OnSettingChanged?.Invoke(id);
		}
		
		public static int GetInt(string id, int fallback) {
			return PlayerPrefs.GetInt(id, fallback);
		}
	}
}

