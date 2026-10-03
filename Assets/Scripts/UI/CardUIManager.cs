using Cards;
using Logic;
using UnityEngine;

namespace UI {
	public class CardUIManager : MonoBehaviour {
		public static CardData selectedCard;

		public void SelectCard() {
			foreach (var modifier in selectedCard.StatModifiers) {
				GameManager.Instance.PlayerStats.AddStatModifier(modifier);
			}
		}
	}
}