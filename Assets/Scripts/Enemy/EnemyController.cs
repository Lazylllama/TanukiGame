using System;
using Player;
using UnityEngine;

namespace Enemy {
	public class EnemyController : MonoBehaviour {
		[SerializeField] private GameObject projectilePrefab;
		[SerializeField] private Vector3    projectileOffset = new Vector3(0f, 0.5f, 0f);

		[SerializeField] private float attackSpeed     = 2f;
		[SerializeField] private float attackMoveSpeed = 1f;

		private float attackTimer;

		private void Update() {
			attackTimer -= Time.deltaTime;
		}

		private void FixedUpdate() {
			if (attackTimer <= 0f) {
				SummonProjectile(PlayerController.Instance.transform.position);
			}
		}

		private void SummonProjectile(Vector3 targetPosition) {
			var targetDirectionRotation = Quaternion.LookRotation(targetPosition - transform.position);

			Instantiate(projectilePrefab,
			            transform.position + (targetPosition.x > transform.position.x
				                                  ? projectileOffset
				                                  : -projectileOffset),
			            targetDirectionRotation);
			attackTimer = attackSpeed;
		}
	}
}