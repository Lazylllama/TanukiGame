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
		[SerializeField] private float returnSpeed;
		[SerializeField] private float positionChangeFrequency;
		[SerializeField] private float hoverPositionAmountX;
		[SerializeField] private float hoverPositionAmountY;


		[Header("Recoil Settings")]
		[SerializeField] private float recoilForce;
		[SerializeField] private float recoilLength;

		//Getters and Setters
		[field: SerializeField] public EnemyStates CurrentEnemyState { get; set; }

		//Private floats
		private float attackTimer;
		private float positionChangeTimer;

		//Private bools
		private bool isLookingRight;
		private bool isAwakened;


		//Private vectors
		private Vector3 currentPosition;
		private Vector3 targetPosition;
		private Vector3 previousPosition;

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
			//Hover();
			Shooting();
			HandleCooldowns();
			SpriteFlip();
		}

		/*private void Hover() {
			positionChangeTimer -= Time.deltaTime;
			if (positionChangeTimer <= 0) {
				positionChangeTimer = positionChangeFrequency;

				targetPosition += new Vector3(Random.Range(hoverPositionAmountX, -hoverPositionAmountX),
				                              Random.Range(hoverPositionAmountY, -hoverPositionAmountY));
			}

			targetPosition = Vector2.MoveTowards(targetPosition, Vector3.zero, returnSpeed * Time.deltaTime);
			currentPosition = Vector2.MoveTowards(currentPosition, targetPosition,
			                                      hoverSpeed * Time.deltaTime);
			var positionToAdd = currentPosition - previousPosition;

			transform.position += positionToAdd;

			previousPosition = currentPosition;
		}*/

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

		private bool PlayerDetected() {
			if (!(Vector2.Distance(transform.position, playerTransform.position) < detectionRange)) return false;
			var hit = Physics2D.Linecast(transform.position, playerTransform.position,
			                             ~LayerMask.GetMask("Enemy"));

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