using UnityEngine;

namespace RNG {
	[CreateAssetMenu(fileName = "RngTables", menuName = "Scriptable Objects/RngTables")]
	class RngTables : ScriptableObject {
		[SerializeField] private ItemTable testTable;
		public ItemTable TestTable => testTable;
	}
}

