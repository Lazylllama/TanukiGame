using System.Collections;
using UnityEngine;

public static class Lib {
	public static class Combat {
		private static IEnumerator PreformedKnockback(Rigidbody2D rigidBody, Vector2 knockbackDirection,
		                                              float       knockbackForce,
		                                              float       knockbackLength) {
			var knockbackTimer = knockbackLength;

			while (knockbackTimer > 0f) {
				knockbackTimer -= Time.deltaTime;

				rigidBody.linearVelocity = knockbackDirection * knockbackForce;

				yield return null;
			}

			yield return null;
		}
	}
}