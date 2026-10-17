using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Logic {
	[Serializable]
	public struct IntSetting {
		public string id;
		public int    value;
	}

	[Serializable]
	public struct BoolSetting {
		public string id;
		public bool   value;
	}

	[Serializable]
	public struct StringSetting {
		public string id;
		public string value;
	}

	[Serializable]
	public class SaveData {
		public int                 totalXp;
		public List<string>        unlockedSkills = new();
		public List<IntSetting>    intSettings    = new();
		public List<BoolSetting>   boolSettings   = new();
		public List<StringSetting> stringSettings = new();
		public bool                tutorialCompleted;
	}

	public static class SaveSystem {
		private static string FilePath => Path.Combine(Application.persistentDataPath, "saveData.json");

		private static SaveData data;
		public static  SaveData Data => data ??= Load();

		public static void Save() {
			//? temp to not blow up the entire save file if something goes to shite
			var temp = FilePath + ".tmp";
			File.WriteAllText(temp, JsonUtility.ToJson(Data, true));

			if (File.Exists(FilePath)) File.Replace(temp, FilePath, null);
			else File.Move(temp, FilePath);
		}

		private static SaveData Load() {
			if (!File.Exists(FilePath)) return new SaveData();

			try {
				return JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath)) ?? new SaveData();
			} catch (Exception e) {
				Debug.LogError($"Failed to load save data from {FilePath}. Returning new SaveData. Exception: {e}");
				return new SaveData();
			}
		}
	}

	public static class Progression {
		//? Make higher = more xp need for level up
		private const float XpScale = 100f;

		public static int Level       => LevelFromXp(SaveSystem.Data.totalXp);
		public static int SkillPoints => Level - 1 - SaveSystem.Data.unlockedSkills.Count;

		public static int LevelFromXp(float xp)    => Mathf.FloorToInt(Mathf.Sqrt(xp / XpScale)) + 1;
		public static int XpForLevel(int    level) => Mathf.RoundToInt(XpScale * (level - 1) * (level - 1));

		public static bool HasSkill(string id) => SaveSystem.Data.unlockedSkills.Contains(id);

		public static void AddXp(float xp) {
			SaveSystem.Data.totalXp += Mathf.RoundToInt(xp);
			SaveSystem.Save();
		}

		public static bool TryUnlockSkill(string id) {
			if (SkillPoints <= 0 || HasSkill(id)) return false;

			SaveSystem.Data.unlockedSkills.Add(id);
			SaveSystem.Save();
			return true;
		}
	}
}