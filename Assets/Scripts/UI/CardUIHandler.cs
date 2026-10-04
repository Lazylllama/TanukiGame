using System;
using Cards;
using Logic;
using UnityEngine;

namespace UI {
	public class CardUIHandler : MonoBehaviour {
		public CardData selectedCard;

		public void SelectCard() {
			foreach (var modifier in selectedCard.StatModifiers) {
				Debug.Log($"adding {modifier.stat} with flat {modifier.flat} and percent {modifier.percent}");
				GameManager.Instance.PlayerStats.AddStatModifier(modifier);
			}

			GameUIManager.Instance.HideCardUI();
		}
	}
}