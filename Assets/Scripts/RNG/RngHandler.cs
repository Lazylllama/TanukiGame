using System;
using System.Linq;
using Cards;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RNG {
	[Serializable]
	public struct Item {
		[SerializeField] public CardType name;
		[SerializeField] public int      weight;
	}

	[Serializable]
	public struct ItemTable {
		[SerializeField] public string name;
		[SerializeField] public Item[] items;

		public int GetWeightSum() {
			return items.Sum(item => item.weight);
		}
	}

	public class RngHandler {
		private readonly RngTables tables;

		public RngHandler(RngTables tables) {
			this.tables = tables;
		}

		public Item RollTable(string tableName, float luck) {
			var table = GetTableByName(tableName);
			if (table.name == null) {
				Debug.Log("Failed to find table " + tableName);
				return new Item();
			}

			var value = Random.value;
			value =  Mathf.Pow(value, 1 / (luck));
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