using System;
using System.Collections;
using System.Collections.Generic;
using Logic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player {
}

public class PlayerCombat : MonoBehaviour {
	public static PlayerCombat Instance;

	[Header("Slash Settings")]
	[SerializeField] private float slashRadius;
	[SerializeField] private float slashDistance;
	[SerializeField] private float slashCooldown;
	[SerializeField] private float slashRecoilForce;
	[SerializeField] private float slashRecoilDuration;
	[SerializeField] private float slashKnockbackForce;
	[SerializeField] private float slashKnockbackLength;
	[SerializeField] private int   slashDamage;

	[Header("Ranged Attack Settings")]
	[SerializeField] private float rangedAttackChargeSpeed;
	[SerializeField] private float rangedAttackMinForce;
	[SerializeField] private float rangedAttackMaxForce;
	[SerializeField] private float rangedAttackCooldown;
	[SerializeField] private int   rangedAttackDamage;


	[Header("Parry Settings")]
	[SerializeField] private float parryLength;
	[SerializeField] private float parryRadius;
	[SerializeField] private float parryCooldown;
	[SerializeField] private float parryRecoilForce;
	[SerializeField] private float parryRecoilDuration;


	[Header("Drag and Drop")]
	[SerializeField] private GameObject slashVFX;

	//Private floats
	private float slashTimer;
	private float parryTimer;
	private float rangedAttackTimer;

	//Getters and setters
	public bool isParrying { get; private set; }

	//Private bools
	private bool rangedAttackHeld;

	//Private vectors
	private Vector2 lookVector;

	//Private layers
	private LayerMask enemyLayer;
	private LayerMask parriableLayer;

	//Components
	private Rigidbody2D playerRb;

	//Coroutines
	private Coroutine parryCoroutine;

	private void Awake() {
		if (Instance != null) {
			Destroy(this.gameObject);
		} else {
			Instance = this;
		}

		enemyLayer     = LayerMask.GetMask("Enemy");
		parriableLayer = LayerMask.GetMask("Parriable");
		playerRb       = GetComponent<Rigidbody2D>();
	}

	private void Update() {
		slashTimer -= Time.deltaTime;
		parryTimer -= Time.deltaTime;
	}

	private void SlashAttack() {
		var slashPosition =
			new Vector2((PlayerController.Instance.IsLookingRight
				             ? slashDistance
				             : -slashDistance) + transform.position.x,
			            transform.position.y);

		var spawnedVfx = Instantiate(slashVFX, slashPosition, slashVFX.transform.rotation);
		spawnedVfx.transform.parent = transform;
		spawnedVfx.SetActive(true);
		Destroy(spawnedVfx, 0.1f);

		var enemies = Physics2D.OverlapCircleAll(slashPosition, slashRadius, enemyLayer);

		if (enemies.Length == 0f) return;

		var recoilDirection = new Vector2(PlayerController.Instance.IsLookingRight ? -1 : 1, 0f);
		StartCoroutine(ExtraForce(slashRecoilForce, recoilDirection, slashRecoilDuration));

		foreach (var enemy in enemies) {
			enemy.GetComponent<EnemyHealth>().ChangeHealth(-slashDamage);
			var knockbackDirection = Mathf.Sign(enemy.transform.position.x - transform.position.x);
			StartCoroutine(Lib.Combat.PreformedKnockback(enemy.GetComponent<Rigidbody2D>(), knockbackDirection,
			                                             slashKnockbackForce, slashKnockbackLength));
		}
	}

	private void RangedAttack() {
		var forceToApply = rangedAttackMinForce;
		var readyToFire  = false;
		if (rangedAttackHeld) {
			if (forceToApply < rangedAttackMaxForce) {
			}
		} else {
		}
	}


	private IEnumerator ExtraForce(float force, Vector2 direction, float duration) {
		while (duration > 0) {
			duration -= Time.deltaTime;

			PlayerController.Instance.ExtraForce = force * direction;

			yield return null;
		}

		PlayerController.Instance.ExtraForce = Vector2.zero;
		yield return null;
	}

	private IEnumerator Parry() {
		isParrying = true;
		var parryHit         = false;
		var parryLengthTimer = parryLength;

		playerRb.linearVelocityX = 0f;

		while (parryLengthTimer > 0f) {
			parryLengthTimer -= Time.deltaTime;
			var parriedColliders = Physics2D.OverlapCircleAll(transform.position, parryRadius, parriableLayer);
			if (parriedColliders.Length > 0) {
				parryHit         = true;
				parryLengthTimer = 0f;
			}

			yield return null;
		}

		isParrying = false;

		if (parryHit) StartCoroutine(ExtraForce(parryRecoilForce, Vector2.up, parryRecoilDuration));

		yield return null;
	}


	private void OnSlash(InputValue value) {
		if (!value.isPressed || !(slashTimer <= 0f)) return;

		slashTimer = slashCooldown;
		SlashAttack();
	}

	private void OnParry(InputValue value) {
		if (!value.isPressed || !(parryTimer <= 0f)) return;
		parryCoroutine = StartCoroutine(Parry());
		parryTimer     = parryCooldown;
	}

	private void OnRangedAttack(InputValue value) {
		if (rangedAttackTimer != 0f) return;
		rangedAttackHeld = value.isPressed;
	}


	private void OnDrawGizmos() {
		Gizmos.color = Color.red;
		if (!PlayerController.Instance) return;
		var slashPosition =
			new Vector2(PlayerController.Instance.IsLookingRight ? 1 : -1 * slashDistance + transform.position.x,
			            transform.position.y);
		Gizmos.DrawWireSphere(slashPosition, slashRadius);
	}
}