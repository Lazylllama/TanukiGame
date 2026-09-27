using System.Collections;
using Enemy;
using Player;
using UnityEngine;

public static class Lib {
	public static class Combat {
		public static IEnumerator PreformedKnockback(Rigidbody2D rigidBody, GameObject gameObject,
		                                             float       knockbackDirectionX,
		                                             float       knockbackForce,
		                                             float       knockbackLength) {
			switch (gameObject.tag) {
				case "Player":
					var playerController = gameObject.GetComponent<PlayerController>();
					playerController.KnockbackActive = true;
					break;
				case "LanternEnemy":
					var lanternEnemy = gameObject.GetComponent<LanternEnemy>();
					lanternEnemy.IsKnockbackActive = true;
					break;
				default: Debug.Log("Oopsie");
					break;
				
			}

			var knockbackTimer = knockbackLength;

			while (knockbackTimer > 0f) {
				knockbackTimer -= Time.deltaTime;

				if (!rigidBody) break;
				rigidBody.linearVelocity = new Vector2(knockbackDirectionX, 1f) * knockbackForce;

				yield return new WaitForEndOfFrame();
			}

			switch (gameObject.tag) {
				case "Player":
					var playerController = gameObject.GetComponent<PlayerController>();
					playerController.KnockbackActive = false;
					break;
				case "LanternEnemy":
					var lanternEnemy = gameObject.GetComponent<LanternEnemy>();
					lanternEnemy.IsKnockbackActive = false;
					break;
				default: Debug.Log("Oopsie");
					break;
				
			}
			
			

			yield return null;
		}

		public static IEnumerator TimeStop(float timeStopLength) {
			var timeStopTimer = timeStopLength;

			while (timeStopTimer > 0f) {
				timeStopTimer  -= Time.unscaledDeltaTime;
				Time.timeScale =  0f;
				yield return null;
			}

			Time.timeScale = 1f;
			yield return null;
		}
	}
}