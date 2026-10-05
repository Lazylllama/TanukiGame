using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Cards {
	public class UpgradeCardRefs : MonoBehaviour {
		[SerializeField] public TextMeshProUGUI cardName;
		[SerializeField] public TextMeshProUGUI cardDescription;
		[SerializeField] public Image           cardImage;
		[SerializeField] public Image           cardBackground;
		[SerializeField] public CardUIHandler   cardUIHandler;
	}
}