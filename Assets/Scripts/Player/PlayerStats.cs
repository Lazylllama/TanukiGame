using System;
using System.Collections.Generic;
using UnityEngine;

namespace Player {
	public enum Stat {
		XpGain,
		Luck,
		CritChance,
		MoveSpeed,
		ExtraHearts,
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
		private readonly Dictionary<Stat, float> flatModifiers    = new Dictionary<Stat, float>();
		private readonly Dictionary<Stat, float> percentModifiers = new Dictionary<Stat, float>();

		public event Action<Stat> OnStatsChanged;

		public void AddStatModifier(StatModifier modifier) {
			flatModifiers[modifier.stat]    = flatModifiers.GetValueOrDefault(modifier.stat)    + modifier.flat;
			percentModifiers[modifier.stat] = percentModifiers.GetValueOrDefault(modifier.stat) + modifier.percent;
			OnStatsChanged?.Invoke(modifier.stat);
		}

		public float Get(Stat stat, float baseValue) {
			return (baseValue + flatModifiers.GetValueOrDefault(stat)) *
			       (1f        + (percentModifiers.GetValueOrDefault(stat) / 100f));
		}
	}
}