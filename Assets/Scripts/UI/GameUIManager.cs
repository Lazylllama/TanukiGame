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

		[Header("Heart juice")]
		[SerializeField] private float heartDrainTime = 0.4f;
		[SerializeField] private float heartPunch = 0.4f;

		private CardUpgradeManager cardUpgradeManager;
		private Action             onCardUpgrade;
		private Color              defaultTimerColor;

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
			defaultTimerColor  = timerText.color;
			UpdateHeartsUI(false);
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

		public void UpdateHeartsUI(bool animate = true) {
			var playerHearts = GameManager.Instance.playerHealth;

			for (var i = 0; i < heartBorderImages.Length; i++) {
				//? Shows always 3 containers, if health is over 3, then show what is needed
				heartBorderImages[i].enabled = i <= 2 || playerHearts > i;
			}

			for (var i = 0; i < heartFillImages.Length; i++) {
				var image  = heartFillImages[i];
				var target = Mathf.Clamp01(playerHearts - i);

				if (!animate) {
					image.fillAmount = target;
					continue;
				}

				if (Mathf.Approximately(image.fillAmount, target)) continue;

				LeanTween.cancel(image.gameObject);
				LeanTween.value(image.gameObject, image.fillAmount, target, heartDrainTime)
				         .setEaseOutQuad()
				         .setOnUpdate(value => image.fillAmount = value);

				Lib.Feel.Punch(image.transform.parent, Vector3.one, heartPunch);
			}
		}

		public void UpdateTimerUI(float timeLeft) {
			timerText.enabled = true;

			var tenths  = Mathf.CeilToInt(Mathf.Max(0f, timeLeft) * 10f);
			var minutes = tenths        / 600f;
			var seconds = tenths % 600f / 10f;

			timerText.text = $"{minutes:00}:{seconds:00.0}";

			timerText.color = tenths == 0 ? new Color(1f, 0.4f, 0.4f) : defaultTimerColor;
		}

		#endregion
	}
}