using System;
using UnityEngine;

namespace Effects {
	[Serializable]
	struct RainInputs {
		[SerializeField] public float fallSpeed, windSpeed, rainPerSecondPerUnit;
		
		public bool Approximately(RainInputs other) {
			return Mathf.Approximately(fallSpeed,            other.fallSpeed) &&
			       Mathf.Approximately(windSpeed,            other.windSpeed) &&
			       Mathf.Approximately(rainPerSecondPerUnit, other.rainPerSecondPerUnit);
		}
	}
	
	
	class RainWindowController : MonoBehaviour
	{
		#region Fields
	
		//? Settings 

		[SerializeField] private bool  receiveInputsFromWeatherSystem = true;
	
		[SerializeField] private RainInputs rainInput;
		private                  RainInputs oldRainInput;

		//? Refs

		[SerializeField] private ParticleSystem rainSystem;
		[SerializeField] private Transform      rainArea;

		#endregion

		#region Unity Functions

		private void Start() {
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
			rainSystem.transform.position = rainArea.position;
			var center    = rainArea.position;
			var scaleX    = rainArea.localScale.x;
			var scaleY    = rainArea.localScale.y;
			var perimeter = Mathf.Sqrt(scaleX * scaleX + scaleY * scaleY);
			var offset    = new Vector2(rainInput.windSpeed, -rainInput.fallSpeed);
			offset.Normalize();
			offset *= -perimeter / 2f;
		
			var shape = rainSystem.shape;
			var emission = rainSystem.emission;
			var main  = rainSystem.main;
			var worldPos = new Vector3(center.x + offset.x, center.y + offset.y, 0);
			shape.position = new Vector3(offset.x, offset.y, 0);
			shape.scale    = new Vector3(1,                   perimeter,           1);
			main.startSpeed = Mathf.Sqrt(rainInput.windSpeed * rainInput.windSpeed + rainInput.fallSpeed * rainInput.fallSpeed);
		
			var dir      = new Vector2(center.x, center.y) - new Vector2(worldPos.x, worldPos.y);
			var rotation = -Mathf.Atan2(dir.y, dir.x);
			shape.rotation     = new Vector3(rotation * Mathf.Rad2Deg,               90,                  0);
			main.startRotation = rotation;
		
			var rate = perimeter * rainInput.rainPerSecondPerUnit;
			emission.rateOverTime = rate;
			main.startLifetimeMultiplier = perimeter / main.startSpeed.constant;
			main.maxParticles     = Mathf.CeilToInt(rate * main.startLifetimeMultiplier);
		}

		#endregion

		private void OnDrawGizmos() {
			Gizmos.matrix = rainSystem.transform.localToWorldMatrix;
			Gizmos.DrawSphere(rainSystem.shape.position, 0.1f);
		}
	}
}

