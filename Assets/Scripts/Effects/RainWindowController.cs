using System;
using System.Collections.Generic;
using UnityEngine;

namespace Effects {
	[Serializable]
	internal struct RainInputs {
		[SerializeField] public float fallSpeed, windSpeed, rainPerSecondPerUnit;

		public bool Approximately(RainInputs other) {
			return Mathf.Approximately(fallSpeed,            other.fallSpeed) &&
			       Mathf.Approximately(windSpeed,            other.windSpeed) &&
			       Mathf.Approximately(rainPerSecondPerUnit, other.rainPerSecondPerUnit);
		}
	}


	class RainWindowController : MonoBehaviour {
		#region Fields

		//? Settings 

		[SerializeField] private bool receiveInputsFromWeatherSystem = true;

		[SerializeField] private RainInputs rainInput;
		private                  RainInputs oldRainInput;

		//? Refs

		[SerializeField] private ParticleSystem  rainSystem;
		[SerializeField] private List<SpriteMask> rainAreas;
		private Bounds rainBounds;

		#endregion

		#region Unity Functions

		private void Start() {
			rainBounds = rainAreas[0].bounds;
			foreach (var area in rainAreas) {
				rainBounds.Encapsulate(area.bounds);
			}
			
			PositionSystem();
		}

		private void Update() {
			if (rainInput.Approximately(oldRainInput)) return;
			PositionSystem();
			oldRainInput = rainInput;
		}

		#endregion

		#region Custom Functions

		public void SetRainSystem(RainInputs other) => rainInput = receiveInputsFromWeatherSystem ? other : rainInput;

		public RainWindowController(RainInputs rainInput) {
			this.rainInput = rainInput;
		}

		private void PositionSystem() {
			rainSystem.transform.position = rainBounds.center;
			var center    = rainBounds.center;
			var scaleX    = rainBounds.size.x;
			var scaleY    = rainBounds.size.y;
			var diagonal = Mathf.Sqrt(scaleX * scaleX + scaleY * scaleY);
			var offset    = new Vector2(rainInput.windSpeed, -rainInput.fallSpeed);
			offset.Normalize();
			offset *= -diagonal / 2f;

			var shape    = rainSystem.shape;
			var emission = rainSystem.emission;
			var main     = rainSystem.main;
			var worldPos = new Vector3(center.x + offset.x, center.y + offset.y, 0);
			shape.position = new Vector3(offset.x, offset.y,  0);
			shape.scale    = new Vector3(1,        diagonal, 1);
			main.startSpeed =
				Mathf.Sqrt(rainInput.windSpeed * rainInput.windSpeed + rainInput.fallSpeed * rainInput.fallSpeed);

			var dir      = new Vector2(center.x, center.y) - new Vector2(worldPos.x, worldPos.y);
			var rotation = -Mathf.Atan2(dir.y, dir.x);
			shape.rotation     = new Vector3(rotation * Mathf.Rad2Deg, 90, 0);
			main.startRotation = rotation;

			var rate = diagonal * rainInput.rainPerSecondPerUnit;
			emission.rateOverTime        = rate;
			main.startLifetimeMultiplier = diagonal / main.startSpeed.constant;
			main.maxParticles            = Mathf.CeilToInt(rate * main.startLifetimeMultiplier);
		}

		#endregion

		private void OnDrawGizmos() {
			Gizmos.DrawWireCube(rainBounds.center, rainBounds.size);
			Gizmos.matrix = rainSystem.transform.localToWorldMatrix;
			Gizmos.DrawSphere(rainSystem.shape.position, 0.1f);
		}
	}
}