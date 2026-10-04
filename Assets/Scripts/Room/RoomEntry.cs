using System;
using Logic;
using UnityEngine;

namespace Room {
	public class RoomEntry : MonoBehaviour {
		[SerializeField] private SpriteRenderer topSpriteRenderer;
		[SerializeField] private SpriteRenderer bottomSpriteRenderer;

		[SerializeField] private Sprite unlockedTopSprite;
		[SerializeField] private Sprite unlockedBottomSprite;

		private void Start() {
			//! hard coded cause we don't have anything before the door *yet
			UnlockDoor();
		}

		public void UnlockDoor() {
			topSpriteRenderer.sprite    = unlockedTopSprite;
			bottomSpriteRenderer.sprite = unlockedBottomSprite;
		}

		private void OnTriggerEnter2D(Collider2D other) {
			GameManager.Instance.LoadFirstRoom();
		}
	}
}