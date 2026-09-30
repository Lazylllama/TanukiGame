using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cards {
	public class UpgradeCardRefs : MonoBehaviour {
		[SerializeField] public TextMeshProUGUI upgradeCardName;
		[SerializeField] public TextMeshProUGUI upgradeCardDescription;
		[SerializeField] public Image           upgradeCardImage;
		[SerializeField] public Image           upgradeCardBackground;
	}
}