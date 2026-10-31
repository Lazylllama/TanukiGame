using System;
using System.Linq;
using Enemy;
using Logic;
using Player;
using UI;
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

			GameUIManager.Instance.ShowBanner(selectedRoomConfiguration.name, 1f);

			PlayerController.Instance.transform.position = selectedRoomRefs.playerSpawnPoint.transform.position;
			GameManager.Instance.roomTimeLeft            = selectedRoomConfiguration.secondLimit;
		}

		public void OnEnemyDeath() {
			if (FindObjectsByType<EnemyHealth>().All(enemy => enemy.IsDead)) {
				selectedRoomRefs.roomDoor.UnlockDoor();
				GameUIManager.Instance.ShowBanner("VICTORY");
			}
		}
	}
}