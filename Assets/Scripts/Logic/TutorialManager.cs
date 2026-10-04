using Room;
using UnityEngine;

namespace Logic {
	public class TutorialManager : MonoBehaviour {
		[SerializeField] private GameObject[] tutorialEnemies;
		private                  RoomEntry    roomEntry;

		private void Awake() {
			tutorialEnemies = GameObject.FindGameObjectsWithTag("Enemy");
			roomEntry       = FindAnyObjectByType<RoomEntry>();
		}

		private void FixedUpdate() {
			print(tutorialEnemies.Length);

			foreach (var enemy in tutorialEnemies) {
				if (!enemy) {
					tutorialEnemies = GameObject.FindGameObjectsWithTag("Enemy");
				}
			}
			
			if (tutorialEnemies.Length == 0) {
				roomEntry.UnlockDoor();
			}
		}
	}
}