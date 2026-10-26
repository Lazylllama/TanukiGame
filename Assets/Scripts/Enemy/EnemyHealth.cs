using System;
using Effects;
using Logic;
using Player;
using UnityEngine;

namespace Enemy {
	public class EnemyHealth : MonoBehaviour {
		private static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");

		[Header("General settings")]
		[SerializeField] private float maxEnemyHealth;
		[SerializeField] private float flashDuration = 0.15f;

		[SerializeField] private DamageIndicator damageIndicatorPrefab;

		private SpriteRenderer[]      spriteRenderers;
		private MaterialPropertyBlock propertyBlock;
		private float                 flash;

		public  bool  IsDead => currentEnemyHealth <= 0;
		private float currentEnemyHealth;

		private void Awake() {
			currentEnemyHealth = maxEnemyHealth;
			spriteRenderers    = GetComponentsInChildren<SpriteRenderer>();
			propertyBlock      = new MaterialPropertyBlock();
		}

		private void Update() {
			if (flash <= 0f) return;
			flash = Mathf.MoveTowards(flash, 0f, Time.deltaTime / flashDuration);
			SetFlash(flash);
		}

		private void SetFlash(float amount) {
			foreach (var renderer in spriteRenderers) {
				renderer.GetPropertyBlock(propertyBlock);
				propertyBlock.SetFloat(FlashAmount, amount);
				renderer.SetPropertyBlock(propertyBlock);
			}
		}

		public void ChangeHealth(float amount) {
			if (IsDead) return;
			currentEnemyHealth += amount;
			if (amount < 0f && damageIndicatorPrefab)
				DamageIndicator.Spawn(damageIndicatorPrefab, transform.position, -amount);
			flash = 1f;

			if (currentEnemyHealth <= 0) {
				GameManager.Instance.OnEnemyDeath(maxEnemyHealth);
				if (GetComponentInChildren<Destructible>() != null) {
					var dest = GetComponentInChildren<Destructible>();
					dest.gameObject.transform.parent = null;
					dest.Shatter((transform.position - PlayerController.Instance.transform.position).normalized);
				}

				Destroy(gameObject);
			}

			
			if (currentEnemyHealth > maxEnemyHealth) currentEnemyHealth = maxEnemyHealth;
		}
	}
}