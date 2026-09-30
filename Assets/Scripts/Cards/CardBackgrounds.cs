using System;
using UnityEngine;

namespace Cards {
	[Serializable]
	public struct CardBackground {
		[SerializeField] public Sprite   cardBackgroundImage;
		[SerializeField] public CardType cardType;
		[SerializeField] public Color    titleColor;
		[SerializeField] public Color    descriptionColor;
	}

	[Serializable]
	[CreateAssetMenu(fileName = "CardBackgrounds", menuName = "Scriptable Objects/CardBackgrounds")]
	public class CardBackgrounds : ScriptableObject {
		[SerializeField] public CardBackground[] cardBackgroundList;

		public CardBackground GetCardBackground(CardType cardType) {
			foreach (var cardBackground in cardBackgroundList) {
				if (cardBackground.cardType == cardType) {
					return cardBackground;
				}
			}

			return default(CardBackground);
		}
	}
}