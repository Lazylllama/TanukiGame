using System;
using UnityEngine;

namespace Room {
	[Serializable]
	public struct RoomConfigurationData {
		public string         name;
		public GameObject     levelPrefab;
		public RoomDifficulty difficulty;
		public RoomType       type;
		public int            secondLimit;
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