using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Data.Cards;
using Button = UnityEngine.UI.Button;

namespace RNG {
	public class RngTestScript : MonoBehaviour {
		[SerializeField] private RngTables       tables;
		[SerializeField] private bool            testFunction;
		[SerializeField] private TextMeshProUGUI text;
		[SerializeField] private Image           spinButtonBackground;
		[SerializeField] private float           luck;

		private void Start() {
			if (!testFunction) return;
			Test();
		}

		private void Update() {
			if (!testFunction) return;
			Test();
		}

		public void Test() {
			var rolledItem = RngHandler.Instance.RollTable("Test", luck);
			testFunction = false;
			text.text    = rolledItem.name.ToString();
			spinButtonBackground.color = rolledItem.name switch {
				CardType.Common    => Color.grey,
				CardType.Rare      => Color.blue,
				CardType.Epic      => Color.purple,
				CardType.Legendary => Color.orange,
				_                  => spinButtonBackground.color
			};
		}

		public void StressTest() {
			int common = 0, rare = 0, epic = 0, legendary = 0;
			var time   = Time.realtimeSinceStartupAsDouble;
			for (var i = 0; i < 1000000; i++) {
				var item = RngHandler.Instance.RollTable("Test", luck);
				switch (item.name) {
					case CardType.Common:
						common++;
						break;
					case CardType.Rare:
						rare++;
						break;
					case CardType.Epic:
						epic++;
						break;
					case CardType.Legendary:
						legendary++;
						break;
				}

				;
			}

			var duration = Time.realtimeSinceStartupAsDouble - time;
			print(common + " : " + rare + " : " + epic + " : " + legendary + " : " + duration * 1000 + "ms");
		}
	}
}