using System;
using Cards;
using Effects;
using Logic;
using Unity.VisualScripting;
using UnityEngine;

namespace UI {
	public class CardUIHandler : MonoBehaviour {
		public CardData selectedCard;
		static bool cardSelected = false;

		public void SelectCard() {
			if (cardSelected) return;
			foreach (var modifier in selectedCard.StatModifiers) {
				Debug.Log($"adding {modifier.stat} with flat {modifier.flat} and percent {modifier.percent}");
				GameManager.Instance.PlayerStats.AddStatModifier(modifier);
			}

			var cards = FindObjectsByType<CardTilt>(FindObjectsSortMode.None);
			Debug.Log(cards.Length);

			cardSelected = true;
			
			foreach (var card in cards) {
				if (gameObject == card.gameObject) continue;
				card.Burn(() => {
					          if (GameUIManager.Instance) GameUIManager.Instance.HideCardUI();
					          cardSelected = false;
				          });
			}

			
		}
	}
}