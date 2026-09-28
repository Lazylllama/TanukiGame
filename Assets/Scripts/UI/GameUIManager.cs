using System;
using Logic;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
	public class GameUIManager : MonoBehaviour {
		#region Fields

		public static GameUIManager Instance;

		[SerializeField] private GameObject cardCanvas;
		[SerializeField] private Image[]    heartImages;

		#endregion

		#region Unity Functions

		private void Awake() {
			if (Instance != null && Instance != this) {
				Destroy(gameObject);
				return;
			}

			Instance = this;
		}

		#endregion

		#region Public Functions

		public void ShowCardUI() {
			cardCanvas.SetActive(true);
		}

		public void UpdateHeartsUI() {
			for (var i = 0; i < heartImages.Length; i++) {
				heartImages[i].fillAmount = GameManager.Instance.playerHearts - i;
			}
		}

		#endregion
	}
}