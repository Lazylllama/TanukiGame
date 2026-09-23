using System;
using Data.Cards;
using RNG;
using UnityEngine;
using UnityEngine.UI;

namespace Logic {
	public class CardUpgradeManager : MonoBehaviour {
		private static GameObject[] fakeCards;
		private static GameObject[] selectableCards;

		// TODO: implement upgrade states
		[SerializeField] private float           luckMultiplier = 1f;
		[SerializeField] private CardBackgrounds cardBackgrounds;
		[SerializeField] private CardData[]      cardUpgrades;

		private void Awake() {
			fakeCards       = GameObject.FindGameObjectsWithTag("FakeUpgradeCard");
			selectableCards = GameObject.FindGameObjectsWithTag("UpgradeCard");
		}

		private void Start() {
			foreach (var card in fakeCards) {
				card.SetActive(false);
			}

			foreach (var card in selectableCards) {
				card.SetActive(false);
			}

			RollAllCards();
		}

		private void RollCard(GameObject card) {
			var type = RngHandler.Instance.RollTable("CardUpgrade", luckMultiplier);
			var bg   = cardBackgrounds.GetCardBackground(type.name);

			print("Rolled card type: " + type.name + " with weight: " + type.weight);

			card.GetComponent<Image>().sprite = bg.cardBackgroundImage;
			card.SetActive(true);
		}

		private void RollAllCards() {
			foreach (var card in fakeCards) RollCard(card);
			foreach (var card in selectableCards) RollCard(card);
		}
	}
}