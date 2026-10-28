using System;
using Player;
using UnityEngine;

namespace Cards {
	public enum CardType {
		Legendary,
		Epic,
		Rare,
		Uncommon,
		Common
	}

	[Serializable]
	[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
	public class CardData : ScriptableObject {
		[SerializeField]            private string         cardName;
		[SerializeField] [TextArea] private string         cardDescription;
		[SerializeField] [TextArea] private string         cardHoloDescription;
		[SerializeField]            private Sprite         cardImage;
		[SerializeField]            private CardType       cardType;
		[SerializeField]            private StatModifier[] statModifiers;
		[SerializeField]            private StatModifier[] statHoloModifiers;
		[SerializeField]            private int            oneInEveryHolo = 100;

		public string         CardName            => cardName;
		public string         CardDescription     => cardDescription;
		public Sprite         CardImage           => cardImage;
		public string         CardHoloDescription => cardHoloDescription;
		public CardType       CardType            => cardType;
		public StatModifier[] StatModifiers       => statModifiers;
		public StatModifier[] StatHoloModifiers   => statHoloModifiers;
		public int            OneInEveryHolo      => oneInEveryHolo;
	}
}