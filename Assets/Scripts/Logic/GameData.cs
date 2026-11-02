using System;
using System.Collections.Generic;
using Cards;
using Player;
using Room;

namespace Logic {
	[Serializable]
	public struct RoomDetails {
		public RoomConfigurationData roomConfiguration;

		public int   roomIndex;
		public float timeSpent;
	}

	[Serializable]
	public struct GameDataStats {
		public List<RoomDetails> roomDetails;
		public List<CardData>    unlockedCards;

		public int   experienceGained;
		public int   lastClearedRoom;
		public int   totalParries;
		public float totalDamageTaken;
		public float totalDamageDealt;

		public string deathReason;
	}

	public class GameData {
		private GameDataStats Stats = new GameDataStats {
			roomDetails      = new List<RoomDetails>(),
			unlockedCards    = new List<CardData>(),
			experienceGained = 0,
			lastClearedRoom  = 0,
			totalParries     = 0,
			totalDamageTaken = 0f,
			totalDamageDealt = 0f,
			deathReason      = ""
		};

		public GameDataStats GetStats() {
			return Stats;
		}

		public void PlayerDied(string reason) {
			Stats.deathReason = reason;
		}

		public void FinishRoom(RoomConfigurationData roomConfiguration, float timeSpent) {
			Stats.roomDetails.Add(new RoomDetails {
				roomConfiguration = roomConfiguration,
				roomIndex         = Stats.lastClearedRoom,
				timeSpent         = timeSpent
			});

			Stats.lastClearedRoom++;
		}

		public void SelectCard(CardData cardData) {
			Stats.unlockedCards.Add(cardData);
		}

		public void DealDamage(float damage, bool isThrowDamage) {
			damage = GameManager.Instance.PlayerStats.Get(
			                                              isThrowDamage ? Stat.ThrowDamage : Stat.MeleeDamage,
			                                              damage
			                                             );
			Stats.totalDamageDealt += damage;
		}

		public void TakeDamage(float damage) {
			Stats.totalDamageTaken += damage;
		}

		public void Parry() {
			Stats.totalParries++;
		}

		public void GainExperience(int experience) {
			Stats.experienceGained += experience;
		}
	}
}