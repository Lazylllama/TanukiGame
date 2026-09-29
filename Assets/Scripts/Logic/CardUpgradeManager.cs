using System.Linq;
using Data.Cards;
using RNG;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Logic {
	public class CardUpgradeManager : MonoBehaviour {
		#region Fields

		private static GameObject[] fakeCards;
		private static GameObject[] selectableCards;

		[SerializeField] private CardBackgrounds cardBackgrounds;
		[SerializeField] private CardData[]      cardUpgrades;

		#endregion

		#region Unity Functions

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

		#endregion

		#region Functions

		private void RollCardWithLuck(GameObject card, float luck = 1) {
			var type = RngHandler.Instance.RollTable("CardUpgrade", luck);
			var bg   = cardBackgrounds.GetCardBackground(type.name);

			print("Rolled card type: " + type.name + " with weight: " + type.weight);

			var upgradesWithType = cardUpgrades.Where(u => u.CardType == type.name).ToArray();
			var cardRefs         = card.GetComponent<UpgradeCardRefs>();
			var randUpgrade      = Random.Range(0, upgradesWithType.Length);

			cardRefs.upgradeCardName.text         = upgradesWithType[randUpgrade].CardName;
			cardRefs.upgradeCardImage.sprite      = upgradesWithType[randUpgrade].CardImage;
			cardRefs.upgradeCardDescription.text  = upgradesWithType[randUpgrade].CardDescription;
			cardRefs.upgradeCardBackground.sprite = bg.cardBackgroundImage;

			cardRefs.upgradeCardName.color        = bg.titleColor;
			cardRefs.upgradeCardDescription.color = bg.descriptionColor;

			card.SetActive(true);
		}

		public void RollAllCards() {
			foreach (var card in fakeCards) RollCardWithLuck(card, 10f);
			foreach (var card in selectableCards) RollCardWithLuck(card);
		}

		#endregion
	}
}