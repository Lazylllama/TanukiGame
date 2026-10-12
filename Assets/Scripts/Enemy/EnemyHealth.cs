using System;
using Logic;
using UnityEngine;

namespace Enemy {
	public class EnemyHealth : MonoBehaviour {
		[Header("General settings")]
		[SerializeField] private float maxEnemyHealth;

		private SpriteRenderer[] spriteRenderers;

		public  bool  IsDead => currentEnemyHealth <= 0;
		private float currentEnemyHealth;

		private void Awake() {
			currentEnemyHealth = maxEnemyHealth;
			spriteRenderers    = GetComponentsInChildren<SpriteRenderer>();
		}

		public void ChangeHealth(float amount) {
			if (IsDead) return;
			currentEnemyHealth += amount;

			foreach (var renderer in spriteRenderers) {
				renderer.color = Color.red;
				LeanTween.value(renderer.gameObject, Color.red, Color.white, 0.15f);
			}

			if (currentEnemyHealth <= 0) {
				GameManager.Instance.OnEnemyDeath(maxEnemyHealth);
				Destroy(gameObject);
			}

			if (currentEnemyHealth > maxEnemyHealth) currentEnemyHealth = maxEnemyHealth;
		}
	}
}