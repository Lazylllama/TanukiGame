using System.Linq;
using Logic;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Cards {
	public class CardUpgradeManager : MonoBehaviour {
		#region Fields

		[SerializeField] private GameObject[] fakeCards;
		[SerializeField] private GameObject[] selectableCards;

		[SerializeField] private CardBackgrounds cardBackgrounds;
		[SerializeField] private CardData[]      cardUpgrades;

		[SerializeField] private Material holographicCardMaterial;
		[SerializeField] private Material normalCardMaterial;

		[Header("dealer")]
		[SerializeField] private float dealOffset = 900f;
		[SerializeField] private float dealTime    = 0.45f;
		[SerializeField] private float dealStagger = 0.08f;

		private Vector3[] selectableCardHomes;

		#endregion

		#region Unity Functions

		private void Start() {
			selectableCardHomes = selectableCards.Select(card => card.transform.localPosition).ToArray();
			HideCards();

			if (fakeCards.Length == 0 || selectableCards.Length == 0) {
				Debug.LogWarning("missing something arent you");
			}
		}

		#endregion

		#region Functions

		/// <summary>
		/// Rolls all cards in the game.
		/// </summary>
		[ContextMenu("Roll All Cards")]
		public void RollAllCards() {
			foreach (var card in fakeCards)
				RollCardWithLuck(card, 10f);

			for (var i = 0; i < selectableCards.Length; i++) {
				RollCardWithLuck(selectableCards[i], GameManager.Instance.PlayerStats.Get(Stat.Luck, 1f));
				DealIn(selectableCards[i].transform, selectableCardHomes[i], i * dealStagger);
			}
		}

		/// <summary>
		///	Hide all cards in the game.
		/// </summary>
		public void HideCards() {
			foreach (var card in fakeCards) {
				card.SetActive(false);
			}

			foreach (var card in selectableCards) {
				card.SetActive(false);
			}
		}

		/// <summary>
		/// Rolls one card with a given a luck value. Higher luck increases chance of a better upgrade, defaults to 1.
		/// </summary>
		/// <param name="card"></param>
		/// <param name="luck"></param>
		private void RollCardWithLuck(GameObject card, float luck = 1) {
			var type = GameManager.Instance.RngHandler.RollTable("CardUpgrade", luck);
			var bg   = cardBackgrounds.GetCardBackground(type.name);

			var upgradesWithType = cardUpgrades.Where(u => u.CardType == type.name).ToArray();
			var refs             = card.GetComponent<UpgradeCardRefs>();
			var randUpgrade      = Random.Range(0, upgradesWithType.Length);

			var oneInChance   = upgradesWithType[randUpgrade].OneInEveryHolo;
			var isHolographic = oneInChance >= 1 && Random.Range(1, oneInChance + 1) == 1;

			refs.cardName.text    = upgradesWithType[randUpgrade].CardName;
			refs.cardImage.sprite = upgradesWithType[randUpgrade].CardImage;
			refs.cardDescription.text = isHolographic
				                            ? upgradesWithType[randUpgrade].CardHoloDescription
				                            : upgradesWithType[randUpgrade].CardDescription;
			refs.cardBackground.sprite = bg.cardBackgroundImage;

			refs.cardBackground.material = isHolographic ? holographicCardMaterial : normalCardMaterial;
			refs.cardImage.material      = isHolographic ? holographicCardMaterial : normalCardMaterial;

			refs.cardName.color        = bg.titleColor;
			refs.cardDescription.color = bg.descriptionColor;

			if (refs.cardUIHandler != null) {
				refs.cardUIHandler.isHolographic = isHolographic;
				refs.cardUIHandler.selectedCard  = upgradesWithType[randUpgrade];
			}


			card.SetActive(true);
		}

		private void DealIn(Transform card, Vector3 home, float stagger = 0f) {
			card.localPosition = home + Vector3.down * dealOffset;
			LeanTween.moveLocal(card.gameObject, home, dealTime)
			         .setDelay(stagger)
			         .setEaseOutBack()
			         .setIgnoreTimeScale(true);
		}

		#endregion
	}
}