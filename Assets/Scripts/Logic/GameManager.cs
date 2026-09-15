using System;
using UnityEngine;

namespace Logic {
	public class GameManager : MonoBehaviour {
		//* Timers
		private float time;

		private void Update() {
			UpdateTimers();
		}

		private void UpdateTimers() {
			time += Time.deltaTime;
		}
	}
}