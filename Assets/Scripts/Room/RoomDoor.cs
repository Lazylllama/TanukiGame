using System;
using Logic;
using UnityEngine;

namespace Room {
	public class RoomDoor : MonoBehaviour {
		[SerializeField] private SpriteRenderer topSpriteRenderer;
		[SerializeField] private SpriteRenderer bottomSpriteRenderer;

		[SerializeField] private Sprite unlockedTopSprite;
		[SerializeField] private Sprite unlockedBottomSprite;

		private bool isUnlocked;
		private bool entered;

		public void UnlockDoor() {
			topSpriteRenderer.sprite    = unlockedTopSprite;
			bottomSpriteRenderer.sprite = unlockedBottomSprite;
			isUnlocked                  = true;
		}

		private void OnTriggerEnter2D(Collider2D other) {
			if (!other.CompareTag("Player") || !isUnlocked || entered) return;
			entered = true;
			if (!GameManager.Instance.inRoom) GameManager.Instance.LoadRoom();
			else GameManager.Instance.FinishRoom();
		}
	}
}