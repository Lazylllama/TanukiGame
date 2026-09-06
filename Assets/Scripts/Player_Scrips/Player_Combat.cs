using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Combat : MonoBehaviour {
	[Header("Slash Settings")]
	[SerializeField] private float slashRadius;
	[SerializeField] private float slashDistance;
	[SerializeField] private float slashCooldown;
	[SerializeField] private float slashRecoilForce;
	[SerializeField] private float slashRecoilDuration;
	[SerializeField] private int   slashDamage;

	[Header("Parry Settings")]
	[SerializeField] private float parryLength;
	[SerializeField] private float parryCooldown;
	[SerializeField] private float parryRadius;
	[SerializeField] private float parryRecoilForce;


	[Header("Drag and Drop")]
	[SerializeField] private GameObject slashVFX;

	//Private floats
	private float slashTimer;
	private float parryTimer;

	//Private vectors
	private Vector2 lookVector;

	//Private layers
	private LayerMask enemyLayer;
	
	//Components
	private Rigidbody2D playerRb;

	private void Awake() {
		enemyLayer = LayerMask.GetMask("Enemy");
		playerRb = GetComponent<Rigidbody2D>();
	}

	private void Update() {
		slashTimer -= Time.deltaTime;
	}

	private void SlashAttack() {
		var slashPosition =
			new Vector2(Player_Controller.Instance.FacingDirection * slashDistance + transform.position.x,
			            transform.position.y);

		var spawnedVfx = Instantiate(slashVFX, slashPosition, slashVFX.transform.rotation);
		spawnedVfx.transform.parent = transform;
		spawnedVfx.SetActive(true);
		Destroy(spawnedVfx, 0.2f);

		var enemies = Physics2D.OverlapCircleAll(slashPosition, slashRadius, enemyLayer);

		if (enemies.Length == 0f) return;

		var recoilDirection = new Vector2(Player_Controller.Instance.FacingDirection * -1, playerRb.linearVelocity.y);  
		StartCoroutine(ExtraForce(slashRecoilForce, recoilDirection, slashRecoilDuration));

		foreach (var enemy in enemies) {
			enemy.GetComponent<Enemy_Health>().ChangeHealth(-slashDamage);
		}
	}

	private IEnumerator ExtraForce(float force, Vector2 direction, float duration) {
		while (duration > 0) {
			duration -= Time.deltaTime;

			
			Player_Controller.Instance.ExtraForce = force * direction;

			yield return null;
		}

		Player_Controller.Instance.ExtraForce = Vector2.zero;
		yield return null;
	}

	private IEnumerator Parry() {
		
	}


	private void OnSlash(InputValue value) {
		if (!value.isPressed || !(slashTimer <= 0f)) return;

		slashTimer = slashCooldown;
		SlashAttack();
	}

	private void OnParry(InputValue value) {
		if (!value.isPressed || !(parryRadius <= 0f)) return;
		parryTimer = parryCooldown;
	}

	private void OnLook(InputValue value) {
		lookVector = value.Get<Vector2>();
		lookVector.Normalize();
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.red;
		var slashPosition =
			new Vector2(Player_Controller.Instance.FacingDirection * slashDistance + transform.position.x,
			            transform.position.y);
		Gizmos.DrawWireSphere(slashPosition, slashRadius);
	}
}