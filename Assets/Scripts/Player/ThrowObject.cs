using System;
using Enemy;
using Unity.VisualScripting;
using UnityEngine;

public class ThrowObject : MonoBehaviour {
	[Header("ThrowObject Settings")]
	[SerializeField] private float throwDamage;
	[SerializeField] private float throwKnockbackForce;
	[SerializeField] private float throwKnockbackLength;

	private void OnTriggerEnter2D(Collider2D enemy) {
		if (enemy.gameObject.layer == LayerMask.GetMask("Enemy")) {
			Debug.Log("Enemy hit with ranged object");
			enemy.GetComponent<EnemyHealth>().ChangeHealth(-throwDamage);

			var knockbackDirection = Mathf.Sign(enemy.transform.position.x - transform.position.x);
			StartCoroutine(Lib.Combat.PreformedKnockback(enemy.GetComponent<Rigidbody2D>(), knockbackDirection,
			                                             throwKnockbackForce, throwKnockbackLength));
		}
	}
}