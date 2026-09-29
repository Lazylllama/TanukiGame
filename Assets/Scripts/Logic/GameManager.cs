using System;
using Player;
using UI;
using UnityEngine;

namespace Logic {
	public class GameManager : MonoBehaviour {
		[SerializeField] private float playerDefaultHearts = 3f;

		public static GameManager Instance;

		public float playerHearts;

		private void Awake() {
			if (Instance != null && Instance != this) {
				Destroy(gameObject);
				return;
			}

			Instance = this;
		}

		private void Start() {
			playerHearts = playerDefaultHearts;
		}

		private void FixedUpdate() {
			GameUIManager.Instance.UpdateHeartsUI();
		}

		public void FinishRoom() {
			PlayerController.Instance.MovementDisabled = true;
			GameUIManager.Instance.ShowCardUI();
		}
	}
}