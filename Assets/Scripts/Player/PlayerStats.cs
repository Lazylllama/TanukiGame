using System;
using System.Collections.Generic;
using UnityEngine;

namespace Player {
	public enum Stat {
		XpGain,
		Luck,
		CritChance,
		MoveSpeed,
		MaxHearts,
		ThrowDamage,
		MeleeDamage
	}

	[Serializable]
	public struct StatModifier {
		public Stat  stat;
		public float flat;
		public float percent;
	}

	public class PlayerStats {
		private readonly Dictionary<Stat, float> flatModifiers    = new();
		private readonly Dictionary<Stat, float> percentModifiers = new();

		public event Action OnStatsChanged;

		public void AddStatModifier(StatModifier modifier) {
			flatModifiers[modifier.stat]    += modifier.flat;
			percentModifiers[modifier.stat] += modifier.percent;
			OnStatsChanged?.Invoke();
		}

		public float Get(Stat stat, float baseValue) {
			return (baseValue + flatModifiers.GetValueOrDefault(stat)) *
			       (1f        + (percentModifiers.GetValueOrDefault(stat) / 100f));
		}
	}
}