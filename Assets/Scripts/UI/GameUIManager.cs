using System;
using Cards;
using Logic;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
	public class GameUIManager : MonoBehaviour {
		#region Fields

		public static GameUIManager Instance;

		[SerializeField] private Image[] heartBorderImages;
		[SerializeField] private Image[] heartFillImages;

		private CardUpgradeManager cardUpgradeManager;
		private Action             onCardUpgrade;

		#endregion

		#region Unity Functions

		private void Awake() {
			if (Instance != null && Instance != this) {
				Destroy(gameObject);
				return;
			}

			Instance = this;
		}

		private void Start() {
			cardUpgradeManager = FindAnyObjectByType<CardUpgradeManager>();
			UpdateHeartsUI();
		}

		#endregion

		#region Public Functions

		[ContextMenu("Show Card UI")]
		public void ShowCardUI(Action callAction = null) {
			cardUpgradeManager.RollAllCards();
			onCardUpgrade = callAction;
		}

		public void HideCardUI() {
			cardUpgradeManager.HideCards();
			onCardUpgrade?.Invoke();
			onCardUpgrade = null;
		}

		public void UpdateHeartsUI() {
			var maxHearts    = GameManager.Instance.MaxHearts;
			var playerHearts = GameManager.Instance.playerHealth;

			for (var i = 0; i < heartFillImages.Length; i++) {
				heartFillImages[i].fillAmount = playerHearts - i;
			}

			for (var i = 0; i < heartBorderImages.Length; i++) {
				//? Shows always 3 containers, if health is over 3, then show what is needed
				heartBorderImages[i].enabled = i <= 2 || playerHearts >= (i + 1);
			}
		}

		#endregion
	}
}