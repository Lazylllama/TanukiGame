using System.Collections;
using UnityEngine;

namespace Effects {
	public class WeatherController : MonoBehaviour {
		#region Fields

		[SerializeField] private float minDelay = 4.0f, maxDelay = 12.0f;

		[SerializeField] private RainWindowController[] windows;
		[SerializeField] private RainInputs             rainInput;
		[SerializeField] private LightningController    lightningController;
		
		private RainInputs oldRainInput;
		private float      cameraWidth;

		#endregion

		#region Unity Functions

		private void Start() {
			SetAllSystems();

			if (Camera.main != null) {
				var cameraSize = Camera.main.orthographicSize;
				cameraWidth = cameraSize * Screen.width / Screen.height;
			}

			StartCoroutine(StrikeRoutine());
		}

		private void Update() {
			if (rainInput.Approximately(oldRainInput)) return;
			SetAllSystems();
			oldRainInput = rainInput;
		}

		private void OnValidate() {
			if(minDelay > maxDelay) minDelay = maxDelay;
			Debug.LogWarning("Tried to set minDelay to a value lesser than maxDelay, minDelay was set to MaxDelay");
		}

		#endregion

		#region Custom Functions

		private void SetAllSystems() {
			foreach (var window in windows) {
				if (window) window.SetRainSystem(rainInput);
			}
		}

		IEnumerator StrikeRoutine() {
			while (true) {
				yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

				StartCoroutine(lightningController.StrikeSequence(Random.Range(-cameraWidth, cameraWidth)));
			}
			// ReSharper disable once IteratorNeverReturns
		}

		#endregion
	}
}