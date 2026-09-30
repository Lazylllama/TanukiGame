using System;
using UnityEngine;

namespace Room {
	public class RoomRefs : MonoBehaviour {
		[SerializeField] public GameObject playerSpawnPoint;

		private void Awake() {
			if (playerSpawnPoint == null) {
				Debug.LogError($"Player spawn point is not assigned in RoomRefs of {gameObject.name}.");
			}
		}
	}
}