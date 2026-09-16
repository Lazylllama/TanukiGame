using System;
using UnityEditor;
using UnityEngine;

public class RainWindowController : MonoBehaviour
{
	#region Fields
	
	//? Settings 

	[SerializeField] private float fallSpeed = 30, windSpeed,    rainPerSecondPerUnit;
	private                  float oldFallSpeed,   oldWindSpeed, oldRainPerSecondPerUnit;

	//? Refs

	[SerializeField] private ParticleSystem rainSystem;
	[SerializeField] private Transform      rainArea;
	[SerializeField] private Transform      mask;

	#endregion

	#region Unity Functions

	private void Start() {
		PositionSystem();
	}

	private void Update() {
		if (!Mathf.Approximately(fallSpeed, oldFallSpeed) || !Mathf.Approximately(windSpeed, oldWindSpeed)) {
			PositionSystem();
			oldFallSpeed = fallSpeed;
			oldWindSpeed = windSpeed;
		}
	}
	#endregion

	#region Custom Functions

	private void PositionSystem() {
		var center = rainArea.position;
		var scaleX = rainArea.localScale.x;
		var scaleY = rainArea.localScale.y;
		var perimeter = Mathf.Sqrt(scaleX * scaleX + scaleY * scaleY);
		var offset = new Vector2(windSpeed, -fallSpeed);
		offset.Normalize();
		offset *= -perimeter / 2f;
		
		var shape = rainSystem.shape;
		var main  = rainSystem.main;
		shape.position = new Vector3(center.x + offset.x, center.y + offset.y, 0);
		shape.scale    = new Vector3(1,                   perimeter,           1);
		main.startSpeed = Mathf.Sqrt(windSpeed * windSpeed + fallSpeed * fallSpeed);
		
		var   dir = new Vector2(center.x, center.y) - new Vector2(shape.position.x, shape.position.y);
		float rotation = -Mathf.Atan2(dir.y, dir.x);
		shape.rotation = new Vector3(rotation * Mathf.Rad2Deg,               90,                  0);
		main.startRotation = rotation;

		mask = rainArea;
	}

	#endregion

	private void OnDrawGizmos() {
		Gizmos.DrawSphere(rainSystem.shape.position, 0.1f);
	}
}
