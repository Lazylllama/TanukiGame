using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RNG {
	[Serializable]
	struct Item {
		[SerializeField] public string name;
		[SerializeField] public int    weight;
	}
	
	[Serializable]
	struct ItemTable {
		[SerializeField] public  Item[]           items;

		public int GetWeightSum() {
			return items.Sum(item => item.weight);
		}
	}
	
	class RngHandler : MonoBehaviour {
		[SerializeField] public  bool             testFunction;
		[SerializeField] private RngTables tables;

		private void Update() {
			
			if (testFunction) 
			{
				Item testItem = RollTable(tables.TestTable);
				testFunction = false;
				print(testItem.name);
			}
		}
		
		
		public Item RollTable(ItemTable table) {
			var value = Random.value;
			value *= table.GetWeightSum();

			foreach (var item in table.items) {
				if (value <= item.weight) return item;
				value -= item.weight;
			}

			return new Item();
		}
	}
}

