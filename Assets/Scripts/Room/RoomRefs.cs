using System;
using UnityEngine;

namespace Room {
	public class RoomRefs : MonoBehaviour {
		[SerializeField] public GameObject playerSpawnPoint;
		[SerializeField] public RoomDoor   roomDoor;

		private void Awake() {
			if (playerSpawnPoint == null) {
				Debug.LogError($"Player spawn point is not assigned in RoomRefs of {gameObject.name}.");
			}

			if (roomDoor == null) {
				Debug.LogError($"Room door is not assigned in RoomRefs of {gameObject.name}.");
			}
		}
	}
}