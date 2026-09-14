using System.Collections;
using UnityEngine;

public static class Lib {
	public static class Combat {
		public static IEnumerator PreformedKnockback(Rigidbody2D rigidBody, float knockbackDirectionX,
		                                             float       knockbackForce,
		                                             float       knockbackLength) {
			var knockbackTimer = knockbackLength;

			while (knockbackTimer > 0f) {
				knockbackTimer -= Time.deltaTime;

				rigidBody.linearVelocity = new Vector2(knockbackDirectionX, 2) * knockbackForce;

				yield return null;
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