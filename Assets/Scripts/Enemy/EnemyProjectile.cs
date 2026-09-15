using System;
using UnityEngine;

namespace Enemy {
	public class EnemyProjectile : MonoBehaviour {
		[SerializeField] private float timeToLive = 5f;
		[SerializeField] private float damage     = 1f;
		[SerializeField] private float speed      = 5f;

		private float aliveTimer;

		private void Update() {
			aliveTimer += Time.deltaTime;
			transform.Translate(Vector3.forward * (speed * Time.deltaTime));

			if (aliveTimer >= timeToLive) Destroy(gameObject);
		}
	}
}