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
			if (gameObject.CompareTag("LanternEnemy")) {
				var lanternEnemy = gameObject.GetComponent<LanternEnemy>();

				lanternEnemy.CurrentEnemyState = LanternEnemy.EnemyStates.Knockback;
			}

			if (gameObject.CompareTag("Player")) {
				var playerController = gameObject.GetComponent<PlayerController>();

				playerController.KnockbackActive = true;
			}

			var knockbackTimer = knockbackLength;

			while (knockbackTimer > 0f) {
				knockbackTimer -= Time.deltaTime;

				if (rigidBody == null) break;
				rigidBody.linearVelocity = new Vector2(knockbackDirectionX, 1f) * knockbackForce;

				yield return new WaitForEndOfFrame();
			}

			Debug.Log("Hej");


			if (gameObject.CompareTag("Player")) {
				var playerController = gameObject.GetComponent<PlayerController>();

				playerController.KnockbackActive = false;
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