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

		public void UnlockDoor() {
			topSpriteRenderer.sprite    = unlockedTopSprite;
			bottomSpriteRenderer.sprite = unlockedBottomSprite;
			isUnlocked                  = true;
		}

		private void OnTriggerEnter2D(Collider2D other) {
			print($"{isUnlocked} {other.CompareTag("Player")} {GameManager.Instance.inRoom}");
			if (!other.CompareTag("Player") || !isUnlocked) return;
			if (!GameManager.Instance.inRoom) GameManager.Instance.LoadRoom();
			else GameManager.Instance.FinishRoom();
		}
	}
}