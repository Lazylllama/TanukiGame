using System;
using System.Collections;
using Unity.U2D.Physics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemy {
	public class LanternEnemy : MonoBehaviour {
		[Header("Lantern Settings")]
		[SerializeField] private float attackCooldown;
		[SerializeField] private float      fireBallSpeed;
		[SerializeField] private float      attackWindup;
		[SerializeField] private float      detectionRange;
		[SerializeField] private Transform  attackPointTransform;
		[SerializeField] private GameObject fireBall;

		[Header("Hover Settings")]
		[SerializeField] private float hoverSpeed;
		[SerializeField] private float hoverLimit;

		[Header("Recoil Settings")]
		[SerializeField] private float recoilForce;
		[SerializeField] private float recoilLength;

		//Getters and Setters
		[field: SerializeField] public EnemyStates CurrentEnemyState { get; set; }

		//Private floats
		private float attackTimer;
		private float positionChangeTimer;
		private float speedCurve;
		private float CosCurve => Mathf.Cos(speedCurve);

		//Private bools
		private bool isLookingRight;
		private bool isAwakened;


		//Private vectors
		private Vector2 hoverPosition;

		//Component references
		private Rigidbody2D lanternRb;
		private Transform   playerTransform;

		//Coroutines
		private Coroutine shootCoroutine;

		private void Awake() {
			lanternRb       = GetComponent<Rigidbody2D>();
			playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
		}

		private void Update() {
			Hover();
			Shooting();
			HandleCooldowns();
			SpriteFlip();
			StateChanger();
		}

		private void Hover() {
			speedCurve += Time.deltaTime * hoverSpeed;

			hoverPosition.y          = CosCurve      * hoverLimit;
			lanternRb.linearVelocity = hoverPosition * hoverSpeed;
		}

		private void Shooting() {
			if (PlayerDetected()) {
				Debug.Log("Player detected");
				if (attackTimer <= 0f && shootCoroutine == null) {
					shootCoroutine = StartCoroutine(Shoot());
				}
			}
		}

		private void SpriteFlip() {
			if (PlayerDetected()) {
				var playerDirectionX = playerTransform.position.x - transform.position.x;
				isLookingRight = playerDirectionX switch {
					> 0 => true,
					< 0 => false,
					_   => isLookingRight
				};
				var rotator = new Vector3(transform.rotation.x, isLookingRight ? 0f : 180f, transform.rotation.z);
				transform.rotation = Quaternion.Euler(rotator);
			}
		}

		private void HandleCooldowns() {
			attackTimer -= Time.deltaTime;
		}

		private IEnumerator Shoot() {
			yield return new WaitForSeconds(attackWindup);
			Debug.Log("Shoot");
			attackTimer = attackCooldown;

			var shootDirection = (playerTransform.position - attackPointTransform.position).normalized;

			var projectile   = Instantiate(fireBall, attackPointTransform.position, Quaternion.identity);
			var projectileRb = projectile.GetComponent<Rigidbody2D>();
			projectileRb.linearVelocity = shootDirection * fireBallSpeed;
			StartCoroutine(EnemyExtraForce(recoilForce, -shootDirection, recoilLength));


			shootCoroutine = null;
		}

		private IEnumerator EnemyExtraForce(float force, Vector2 direction, float duration) {
			lanternRb.linearVelocity = Vector2.zero;
			while (duration > 0) {
				duration -= Time.deltaTime;

				lanternRb.linearVelocity = force * direction;

				yield return null;
			}

			lanternRb.linearVelocity = Vector2.zero;
			yield return null;
		}

		private void StateChanger() {
			if (isAwakened) CurrentEnemyState = EnemyStates.AwakenedIdle;
		}

		private bool PlayerDetected() {
			if (!(Vector2.Distance(transform.position, playerTransform.position) < detectionRange)) return false;
			var hit = Physics2D.Linecast(transform.position, playerTransform.position,
			                             ~LayerMask.GetMask("Enemy"));
			isAwakened = true;

			return hit.collider;
		}

		public enum EnemyStates {
			Idle,
			AwakenedIdle,
			Awakening,
			Shooting,
			Knockback
		}
	}
}