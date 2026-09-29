using System;
using Data.Rooms;
using Data.Rooms;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Room {
	public class RoomController : MonoBehaviour {
		[SerializeField] private RoomConfigurations roomConfigurations;

		public RoomConfigurationData selectedRoomConfiguration;
		
		private void Awake() {
			selectedRoomConfiguration = roomConfigurations.Data[Random.Range(0, roomConfigurations.Data.Length)];
			Debug.Log($"Selected room configuration: {selectedRoomConfiguration.difficulty}");
		}

		private void Start() {
			Instantiate(selectedRoomConfiguration.levelPrefab, new Vector3(0, 0, 0), Quaternion.identity);
			PlayerController.Instance.transform.position = selectedRoomConfiguration.playerSpawnPosition;
		}
	}
}