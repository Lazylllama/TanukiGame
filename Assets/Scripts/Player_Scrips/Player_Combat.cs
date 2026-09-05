using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Combat : MonoBehaviour {
	[Header("Combat Settings")]
	[SerializeField] private float slashRadius;
	[SerializeField] private float slashDistance;
	[SerializeField] private float slashCooldown;
	[SerializeField] private int   slashDamage;

	[Header("Drag and Drop")]
	[SerializeField] private GameObject slashVFX;

	//Private floats
	private float slashTimer;

	//Private vectors
	private Vector2 lookVector;

	//Private layers
	private LayerMask enemyLayer;

	private void Awake() {
		enemyLayer = LayerMask.GetMask("Enemy");
	}

	private void Update() {
		slashTimer -= Time.deltaTime;
	}

	private void SlashAttack() {
		var slashPosition = new Vector2(lookVector.x * slashDistance + transform.position.x,
		                                lookVector.y * slashDistance + transform.position.y);

		var spawnedVfx = Instantiate(slashVFX, slashPosition, slashVFX.transform.rotation);
		spawnedVfx.transform.parent = transform;
		spawnedVfx.SetActive(true);
		Destroy(spawnedVfx, 0.2f);

		var enemies = Physics2D.OverlapCircleAll(slashPosition, slashRadius, enemyLayer);
		if (enemies == null) return;
		foreach (var enemy in enemies) {
			enemy.GetComponent<Enemy_Health>().ChangeHealth(-slashDamage);
		}
	}


	private void OnSlash(InputValue value) {
		if (!value.isPressed || !(slashTimer <= 0f)) return;

		slashTimer = slashCooldown;
		SlashAttack();
	}

	private void OnLook(InputValue value) {
		lookVector = value.Get<Vector2>();
		lookVector.Normalize();
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.red;
		var slashPosition = new Vector2(lookVector.x * slashDistance + transform.position.x,
		                                lookVector.y * slashDistance + transform.position.y);
		Gizmos.DrawWireSphere(slashPosition, slashRadius);
	}
}