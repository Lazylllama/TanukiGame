using System;
using UnityEngine;

namespace Data.Rooms {
	[Serializable]
	public struct EnemyData {
		public GameObject prefab;
	}

	[Serializable]
	public struct RoomConfigurationData {
		public GameObject     levelPrefab;
		public EnemyData[]    enemies;
		public RoomDifficulty difficulty;
		public RoomType       type;
		public int            minuteLimit;
	}

	public enum RoomDifficulty {
		Easy,
		Medium,
		Hard
	}

	public enum RoomType {
		Parkour,
		Combat
	}

	[Serializable]
	[CreateAssetMenu(fileName = "RoomConfigurations", menuName = "Scriptable Objects/RoomConfigurations")]
	public class RoomConfigurations : ScriptableObject {
		[SerializeField] private RoomConfigurationData[] data;

		public RoomConfigurationData[] Data => data;
	}
}