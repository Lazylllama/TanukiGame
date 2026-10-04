using System;
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

		public void ShowCardUI() {
			cardUpgradeManager.RollAllCards();
		}

		public void HideCardUI() {
			cardUpgradeManager.HideCards();
			PlayerController.Instance.MovementDisabled = false;
		}

		public void UpdateHeartsUI() {
			var defaultHearts = GameManager.Instance.defaultHearts;
			var totalHearts   = GameManager.Instance.PlayerStats.Get(Stat.ExtraHearts, defaultHearts);
			var playerHearts  = GameManager.Instance.playerHearts;

			for (var i = 0; i < heartFillImages.Length; i++) {
				heartFillImages[i].fillAmount = playerHearts - i;
			}

			for (var i = 0; i < heartBorderImages.Length; i++) {
				heartBorderImages[i].enabled = totalHearts >= (i + 1);
			}
		}

		#endregion
	}
}