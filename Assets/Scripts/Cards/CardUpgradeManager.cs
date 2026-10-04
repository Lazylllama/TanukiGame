using System.Linq;
using Cards;
using Player;
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
			HideCards();
		}

		#endregion

		#region Functions

		public void RollAllCards() {
			foreach (var card in fakeCards)
				RollCardWithLuck(card, 10f);

			foreach (var card in selectableCards)
				RollCardWithLuck(card, GameManager.Instance.PlayerStats.Get(Stat.Luck, 1f));
		}

		public void HideCards() {
			foreach (var card in fakeCards) {
				card.SetActive(false);
			}

			foreach (var card in selectableCards) {
				card.SetActive(false);
			}
		}

		private void RollCardWithLuck(GameObject card, float luck = 1) {
			var type = GameManager.Instance.RngHandler.RollTable("CardUpgrade", luck);
			var bg   = cardBackgrounds.GetCardBackground(type.name);

			var upgradesWithType = cardUpgrades.Where(u => u.CardType == type.name).ToArray();
			var refs             = card.GetComponent<UpgradeCardRefs>();
			var randUpgrade      = Random.Range(0, upgradesWithType.Length);

			refs.cardName.text         = upgradesWithType[randUpgrade].CardName;
			refs.cardImage.sprite      = upgradesWithType[randUpgrade].CardImage;
			refs.cardDescription.text  = upgradesWithType[randUpgrade].CardDescription;
			refs.cardBackground.sprite = bg.cardBackgroundImage;

			refs.cardName.color        = bg.titleColor;
			refs.cardDescription.color = bg.descriptionColor;

			if (refs.cardUIHandler != null)
				refs.cardUIHandler.selectedCard = upgradesWithType[randUpgrade];

			card.SetActive(true);
		}

		#endregion
	}
}