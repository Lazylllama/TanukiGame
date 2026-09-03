using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Combat : MonoBehaviour {
	[Header("Combat Settings")]
	[SerializeField] private float slashRadius;
	[SerializeField] private int slashDamage;
	[SerializeField] private int slashCooldown;

	[Header("Drag and Drop")]
	[SerializeField] private Transform slashPoint;

	private float slashTimer;


	private LayerMask enemyLayer;

	private void Awake() {
		enemyLayer = LayerMask.GetMask("Enemy");
	}

	private void SlashAttack() {
		var enemies = Physics2D.OverlapCircleAll(slashPoint.position, slashRadius, enemyLayer);

		if (enemies == null) return;
		foreach (var enemy in enemies) {
		}
	}


	private void OnSlash(InputValue value) {
		if (!value.isPressed || !(slashTimer <= 0f)) return;

		slashTimer = slashCooldown;
		SlashAttack();
	}
}