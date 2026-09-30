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
		public RoomRefs              roomRefs;

		private void Awake() {
			selectedRoomConfiguration = roomConfigurations.Data[Random.Range(0, roomConfigurations.Data.Length)];
			Debug.Log($"Selected room configuration: {selectedRoomConfiguration.difficulty}");
		}

		private void Start() {
			var levelGameObject = Instantiate(selectedRoomConfiguration.levelPrefab, new Vector3(0, 0, 0),
			                                  Quaternion.identity);

			roomRefs = levelGameObject.GetComponent<RoomRefs>();

			PlayerController.Instance.transform.position = roomRefs.playerSpawnPoint.transform.position;
		}
	}
}