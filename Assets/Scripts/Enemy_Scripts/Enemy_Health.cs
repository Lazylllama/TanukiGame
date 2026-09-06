using System;
using UnityEngine;

public class Enemy_Health : MonoBehaviour {
	[Header("General settings")]
	[SerializeField] private float maxEnemyHealth;

	//Private floats
	private float currentEnemyHealth;

	private void Awake() {
		currentEnemyHealth = maxEnemyHealth;
	}

	public void ChangeHealth(float amount) {
		Debug.Log("ChangeHealth");
		currentEnemyHealth += amount;

		if (currentEnemyHealth <= 0) Destroy(gameObject);
		if (currentEnemyHealth > maxEnemyHealth) currentEnemyHealth = maxEnemyHealth;
	}
}