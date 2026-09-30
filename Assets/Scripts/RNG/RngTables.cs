using UnityEngine;

namespace RNG {
	[CreateAssetMenu(fileName = "RngTables", menuName = "Scriptable Objects/RngTables")]
	public class RngTables : ScriptableObject {
		[SerializeField] public ItemTable[] tables;
	}
}