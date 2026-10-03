using System;
using Data.Rooms;
using Data.Rooms;
using Enemy;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Room {
	public class RoomController : MonoBehaviour {
		public static RoomController Instance;

		[SerializeField] private RoomConfigurations roomConfigurations;

		[Header("Refs")]
		public RoomConfigurationData selectedRoomConfiguration;
		public RoomRefs selectedRoomRefs;

		private void Awake() {
			selectedRoomConfiguration = roomConfigurations.Data[Random.Range(0, roomConfigurations.Data.Length)];
			Debug.Log($"Selected room configuration: {selectedRoomConfiguration.difficulty}");

			if (Instance != null && Instance != this) {
				Destroy(gameObject);
				return;
			}

			Instance = this;
		}

		private void Start() {
			var levelGameObject = Instantiate(selectedRoomConfiguration.levelPrefab, new Vector3(0, 0, 0),
			                                  Quaternion.identity);

			selectedRoomRefs = levelGameObject.GetComponent<RoomRefs>();

			PlayerController.Instance.transform.position = selectedRoomRefs.playerSpawnPoint.transform.position;
		}

		public void UnlockEscape() => Debug.Log("unlock escape");
		public void EscapeRoom()   => Debug.Log("escape ze room");
	}
}