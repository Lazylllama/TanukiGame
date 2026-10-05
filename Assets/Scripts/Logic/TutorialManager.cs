using System.Linq;
using Room;
using UnityEngine;

namespace Logic {
	public class TutorialManager : MonoBehaviour {
		private GameObject[] tutorialEnemies;
		private RoomEntry    roomEntry;

		private bool doorUnlocked;

		private void Awake() {
			tutorialEnemies = GameObject.FindGameObjectsWithTag("Enemy");
			roomEntry       = FindAnyObjectByType<RoomEntry>();
		}

		private void FixedUpdate() {
			tutorialEnemies = tutorialEnemies.Where(e => e).ToArray();

			if (tutorialEnemies.Length != 0 || doorUnlocked) return;
			roomEntry.UnlockDoor();
			doorUnlocked = true;
		}
	}
}