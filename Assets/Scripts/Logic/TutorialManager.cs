using System.Linq;
using Room;
using UnityEngine;

namespace Logic {
	public class TutorialManager : MonoBehaviour {
		private GameObject[] tutorialEnemies;
		private RoomDoor    roomDoor;

		private bool doorUnlocked;

		private void Awake() {
			tutorialEnemies = GameObject.FindGameObjectsWithTag("Enemy");
			roomDoor       = FindAnyObjectByType<RoomDoor>();
		}

		private void FixedUpdate() {
			tutorialEnemies = tutorialEnemies.Where(e => e).ToArray();

			if (tutorialEnemies.Length != 0 || doorUnlocked) return;
			roomDoor.UnlockDoor();
			doorUnlocked = true;
		}
	}
}