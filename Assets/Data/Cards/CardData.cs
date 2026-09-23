using System;
using Data.Cards;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data.Cards {
	public enum CardType {
		Legendary,
		Epic,
		Rare,
		Uncommon,
		Common
	}

	[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
	public class CardData : ScriptableObject {
		[SerializeField]            private string   cardName;
		[SerializeField] [TextArea] private string   cardDescription;
		[SerializeField]            private Sprite   cardImage;
		[SerializeField]            private CardType cardType;

		public string   CardName        => cardName;
		public string   CardDescription => cardDescription;
		public Sprite   CardImage       => cardImage;
		public CardType CardType        => cardType;
	}

	[Serializable]
	public struct CardBackground {
		[SerializeField] public Sprite   cardBackgroundImage;
		[SerializeField] public CardType cardType;
		[SerializeField] public Color    titleColor;
		[SerializeField] public Color    descriptionColor;
	}

	[CreateAssetMenu(fileName = "CardBackgrounds", menuName = "Scriptable Objects/CardBackgrounds")]
	public class CardBackgrounds : ScriptableObject {
		[SerializeField] public CardBackground[] cardBackgrounds;

		public CardBackground GetCardBackground(CardType cardType) {
			foreach (var cardBackground in cardBackgrounds) {
				if (cardBackground.cardType == cardType) {
					return cardBackground;
				}
			}

			return default(CardBackground);
		}
	}
}