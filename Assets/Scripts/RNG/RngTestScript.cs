using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace RNG {
	public class RngTestScript : MonoBehaviour {
		[SerializeField] private RngTables       tables;
		[SerializeField] private bool            testFunction;
		[SerializeField] private TextMeshProUGUI text;
		[SerializeField] private Image          spinButtonBackground;
			

		private void Start() {
			if (!testFunction) return;
			Test();
		}

		private void Update() {
			if (!testFunction) return;
			Test();
		}

		public void Test() {
			var rolledItem = RngHandler.Instance.RollTable("Test");
			testFunction = false;
			text.text = rolledItem.name;
			if (rolledItem.name == "common") {
				spinButtonBackground.color = Color.grey;
			} else if (rolledItem.name == "uncommon") {
				spinButtonBackground.color = Color.green;
			} else if (rolledItem.name == "rare") {
				spinButtonBackground.color = Color.blue;
			} else if (rolledItem.name == "epic") {
				spinButtonBackground.color = Color.purple;
			} else if (rolledItem.name == "legendary") {
				spinButtonBackground.color = Color.orange;
			}
		}
	}
}