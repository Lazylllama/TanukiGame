using System;
using UnityEngine;

namespace Player {
	public class ParryOrb : MonoBehaviour {
		[SerializeField] private float spawnDelay            = 2f;
		[SerializeField] private bool  requirePlayerGrounded = true;

		private Collider2D     collider;
		private SpriteRenderer spriteRenderer;

		private void Awake() {
			collider       = GetComponent<Collider2D>();
			spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		}

		public void OnParry() {
			SetActive(false);
			Invoke(nameof(Respawn), spawnDelay);
		}

		private void Respawn() {
			if (!requirePlayerGrounded || PlayerController.Instance.GetIsGrounded()) SetActive(true);
			else Invoke(nameof(Respawn), 0.2f);
		}

		private void SetActive(bool isActive) {
			collider.enabled       = isActive;
			spriteRenderer.enabled = isActive;
		}
	}
}