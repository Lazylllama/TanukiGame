using System;
using Cards;
using Logic;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
	public class GameUIManager : MonoBehaviour {
		#region Fields

		public static GameUIManager Instance;

		[SerializeField] private TextMeshProUGUI timerText;
		[SerializeField] private Image[]         heartBorderImages;
		[SerializeField] private Image[]         heartFillImages;

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
			timerText.enabled  = false;
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
			var playerHearts = GameManager.Instance.playerHealth;

			for (var i = 0; i < heartFillImages.Length; i++) {
				heartFillImages[i].fillAmount = playerHearts - i;
			}

			for (var i = 0; i < heartBorderImages.Length; i++) {
				//? Shows always 3 containers, if health is over 3, then show what is needed
				heartBorderImages[i].enabled = i <= 2 || playerHearts >= (i + 1);
			}
		}

		public void UpdateTimerUI(float timeLeft) {
			timerText.enabled = true;

			var minutes = Mathf.FloorToInt(timeLeft / 60f);
			var seconds = timeLeft - (minutes * 60f);

			timerText.text = $"{minutes:00}:{seconds:F1}";

			if (timeLeft <= 0f) {
				timerText.color = new Color(1f, 0.4f, 0.4f);
			}
		}

		#endregion
	}
}