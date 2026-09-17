using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RNG {
	[Serializable]
	public struct Item {
		[SerializeField] public string name;
		[SerializeField] public int    weight;
	}

	[Serializable]
	public struct ItemTable {
		[SerializeField] public string name;
		[SerializeField] public Item[] items;

		public int GetWeightSum() {
			return items.Sum(item => item.weight);
		}
	}

	public class RngHandler : MonoBehaviour {
		[SerializeField] private RngTables tables;

		public static RngHandler Instance;

		private void Awake() {
			if (Instance == null) Instance = this;
			else Destroy(gameObject);
		}

		public Item RollTable(string tableName) {
			var table = GetTableByName(tableName);
			if (table.name == null) {print("Failed to find table " + tableName); return new Item();};
			var value = Random.value;
			value *= table.GetWeightSum();

			foreach (var item in table.items) {
				if (value <= item.weight) return item;
				value -= item.weight;
			}

			return new Item();
		}

		private ItemTable GetTableByName(string tableName) {
			foreach (var table in tables.tables) {
				if (table.name != tableName) continue;
				return table;
			}
			return new ItemTable();
		}
	}
}