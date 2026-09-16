
using UnityEngine;

namespace Effects {
	public class WeatherController : MonoBehaviour
	{
		#region Fields

		[SerializeField] private RainWindowController[] windows;
		[SerializeField] private RainInputs             rainInput;
		private                  RainInputs             oldRainInput;

		#endregion
		
		#region Unity Functions

		private void Start() {
			SetAllSystems();
		}

		private void Update() {
			if (rainInput.Approximately(oldRainInput)) return;
			SetAllSystems();
			oldRainInput = rainInput;
		}
		
		#endregion

		#region Custom Functions

		private void SetAllSystems() {
			foreach (var window in windows) {
				window.SetRainSystem(rainInput);
			}
		}

		#endregion
	}
}

