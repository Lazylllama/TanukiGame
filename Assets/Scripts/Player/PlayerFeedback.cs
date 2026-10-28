using System;
using System.Collections;
using Logic;
using UnityEngine;

namespace Player {
	public class PlayerFeedback : MonoBehaviour {
		private static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");
		[Header("Player Damaged")]
		[SerializeField] private float hurtCamShakeForce = 1f;
		[SerializeField] private float hitFlashDuration    = 0.2f;
		[SerializeField] private float hitStopLength       = 0.2f;
		[SerializeField] private float invulnFlashInterval = 0.1f;

		private float     hitFlash;
		private Coroutine hurtRoutine;

		private MaterialPropertyBlock propertyBlock;
		private SpriteRenderer[]      spriteRenderers;

		private void Awake() {
			propertyBlock   = new MaterialPropertyBlock();
			spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
		}

		private void Start() {
			GameManager.Instance.OnPlayerDamaged += OnPlayerDamaged;
		}

		private void OnDestroy() {
			if (GameManager.Instance) GameManager.Instance.OnPlayerDamaged -= OnPlayerDamaged;
		}

		private void Update() {
			//* Player Damaged
			if (hitFlash <= 0f) return;
			hitFlash = Mathf.MoveTowards(hitFlash, 0f, Time.deltaTime / hitFlashDuration);
			SetHitFlash(hitFlash);
		}

		#region Player Damaged

		private void OnPlayerDamaged() {
			if (hurtRoutine != null) StopCoroutine(hurtRoutine);
			hurtRoutine = StartCoroutine(Hurt());

			//* lowkey jus feels like the game is lagging rn
			//StartCoroutine(Lib.Feel.TimeStop(hitStopLength));


			CameraManager.Instance.ShakeCamera(hurtCamShakeForce);
			CameraManager.Instance.HurtPulse();
		}

		private IEnumerator Hurt() {
			SetSpritesEnabled(true);

			hitFlash = 1f;
			yield return new WaitForSeconds(hitFlashDuration);

			var blinkTime = GameManager.Instance.invulnerabilityDuration - hitFlashDuration;

			while (blinkTime > 0f) {
				foreach (var renderer in spriteRenderers) {
					renderer.enabled = !renderer.enabled;
				}

				blinkTime -= invulnFlashInterval;
				yield return new WaitForSeconds(invulnFlashInterval);
			}

			SetSpritesEnabled(true);
			hurtRoutine = null;
		}

		private void SetHitFlash(float amount) {
			foreach (var renderer in spriteRenderers) {
				renderer.GetPropertyBlock(propertyBlock);
				propertyBlock.SetFloat(FlashAmount, amount);
				renderer.SetPropertyBlock(propertyBlock);
			}
		}

		#endregion

		private void SetSpritesEnabled(bool newState) {
			foreach (var renderer in spriteRenderers) {
				renderer.enabled = newState;
			}
		}
	}
}