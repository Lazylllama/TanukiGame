using System;
using Enemy;
using Unity.VisualScripting;
using UnityEngine;

namespace Player {
	public class PlayerKnife : MonoBehaviour {
		[Header("ThrowObject Settings")]
		[SerializeField] private float damage;
		[SerializeField] private float knockbackForce;
		[SerializeField] private float knockbackLength;

		private void OnTriggerEnter2D(Collider2D enemy) {
			if (enemy.gameObject.layer == LayerMask.NameToLayer("Enemy")) {
				Debug.Log("Enemy hit with ranged knife");
				enemy.GetComponent<EnemyHealth>().ChangeHealth(-damage);

				var knockbackDirection = Mathf.Sign(enemy.transform.position.x - transform.position.x);
				StartCoroutine(Lib.Combat.PreformedKnockback(enemy.GetComponent<Rigidbody2D>(), enemy.gameObject,
				                                             knockbackDirection,
				                                             knockbackForce, knockbackLength));
				Destroy(gameObject);
			}
		}
	}
}