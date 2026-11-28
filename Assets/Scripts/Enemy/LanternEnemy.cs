using System;
using System.Collections;
using UnityEngine;

namespace Enemy {
	public class LanternEnemy : MonoBehaviour {
		public enum EnemyStates {
			Idle,
			AwakenedIdle,
			Shooting,
			Knockback
		}

		#region Fields

		[Header("Combat Settings")]
		[SerializeField] private float attackCooldown;
		[SerializeField] private float      fireBallSpeed;
		[SerializeField] private float      attackWindup;
		[SerializeField] private float      detectionRange;
		[SerializeField] private Transform  attackPointTransform;
		[SerializeField] private GameObject fireBall;

		[Header("Movement Settings")]
		[SerializeField] private float moveSpeed;
		[SerializeField] private float acceleration;
		[SerializeField] private float deceleration;
		[SerializeField] private float velPower;
		[SerializeField] private float followRadius;
		[SerializeField] private float fleeRadius;
		[SerializeField] private bool  following;
		[SerializeField] private bool  fleeing;

		[Header("Hover Settings")]
		[SerializeField] private float hoverSpeed;
		[SerializeField] private float curveScale;
		[SerializeField] private float curveSpeed;
		[SerializeField] private float time;


		[Header("Recoil Settings")]
		[SerializeField] private float recoilForce;
		[SerializeField] private float recoilLength;
		[SerializeField] private float returnSpeed;

		//? Getters and Setters
		[field: SerializeField] public EnemyStates CurrentEnemyState { get; set; }
		public                         bool        IsKnockbackActive { get; set; }

		//? Private floats
		private float attackTimer;

		//? Private bools
		private bool isLookingRight;
		private bool isAwakened;
		private bool isShooting;

		//? Private vectors
		private Vector2 hoverPosition;

		//? Component references
		private Rigidbody2D lanternRb;
		private Transform   playerTransform;

		//? Coroutines
		private Coroutine shootCoroutine;

		#endregion

		#region Unity Functions

		private void Awake() {
			lanternRb       = GetComponent<Rigidbody2D>();
			playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
		}

		private void Update() {
			//Hover();
			Shooting();
			HandleCooldowns();
			SpriteFlip();
			StateChanger();
		}

		private void FixedUpdate() {
			Movement();
		}

		#endregion

		#region Functions

		private void Hover() {
			time += curveSpeed * Time.deltaTime;

			var curveValueY = Mathf.PerlinNoise(time * -1, 0) * curveScale;

			var curveY = curveValueY * hoverSpeed;


			lanternRb.AddForce(curveY * Vector2.up);
		}

		private void Movement() {
			if (!isAwakened || CurrentEnemyState == EnemyStates.Knockback) return;
			var playerDirection = (playerTransform.position - gameObject.transform.position).normalized;

			if ((Vector2.Distance(transform.position, playerTransform.position) > followRadius)) {
				following = true;
				Debug.Log("Following");
			} else following = false;

			if (Vector2.Distance(transform.position, playerTransform.position) < followRadius &&
			    Vector2.Distance(transform.position, playerTransform.position) > fleeRadius) {
				playerDirection = Vector2.zero;
				Debug.Log("Chill zone");
			}

			if ((Vector2.Distance(transform.position, playerTransform.position) < fleeRadius)) {
				fleeing         =  true;
				playerDirection *= -1;
				Debug.Log("Fleeing");
			} else fleeing = false;


			var movementVector = new Vector2(XYAccelDeccel(playerDirection.x, true),
			                                 XYAccelDeccel(playerDirection.y, false));
			Debug.Log(movementVector);
			lanternRb.AddForce(movementVector);
		}

		private float XYAccelDeccel(float playerDirection, bool xMovement) {
			var targetSpeed = playerDirection * moveSpeed;

			var speedDif = targetSpeed - (xMovement ? lanternRb.linearVelocity.x : lanternRb.linearVelocity.y);

			var accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

			var movement = Mathf.Abs(speedDif) * accelRate * Mathf.Sign(speedDif);
			return movement;
		}


		private void Shooting() {
			if (!isAwakened || following || fleeing) return;
			if (attackTimer <= 0f && shootCoroutine == null) {
				shootCoroutine = StartCoroutine(Shoot());
			}
		}

		private void SpriteFlip() {
			if (!PlayerDetected()) return;
			var playerDirectionX = playerTransform.position.x - transform.position.x;
			isLookingRight = playerDirectionX switch {
				> 0 => true,
				< 0 => false,
				_   => isLookingRight
			};
			var rotator = new Vector3(transform.rotation.x, isLookingRight ? 0f : 180f, transform.rotation.z);
			transform.rotation = Quaternion.Euler(rotator);
		}

		private void HandleCooldowns() {
			attackTimer -= Time.deltaTime;
		}

		private IEnumerator Shoot() {
			isShooting = true;
			yield return new WaitForSeconds(attackWindup);
			attackTimer = attackCooldown;

			var shootDirection = (playerTransform.position - gameObject.transform.position).normalized;

			var projectile   = Instantiate(fireBall, attackPointTransform.position, Quaternion.identity);
			var projectileRb = projectile.GetComponent<Rigidbody2D>();
			projectileRb.linearVelocity = shootDirection * fireBallSpeed;
			isShooting                  = false;

			//StartCoroutine(EnemyExtraForce(recoilForce, -shootDirection, recoilLength));
			//StartCoroutine(LanternRecoil(recoilForce, -shootDirection, recoilLength));


			shootCoroutine = null;
		}

		private IEnumerator LanternRecoil(float force, Vector2 direction, float duration) {
			var forceToApply = 0f;
			var timer        = duration;
			Debug.Log("Entered recoil coroutine");
			var oldPosition = transform.position;
			LeanTween.value(gameObject, 0f, force, duration)
			         .setEaseOutQuart().setOnUpdate(val => { forceToApply = val; });

			while ((transform.position != oldPosition || timer > 0f) && timer > -1f) {
				timer -= Time.deltaTime;

				while (LeanTween.isTweening(gameObject)) {
					lanternRb.linearVelocity = direction * forceToApply;
					yield return new WaitForEndOfFrame();
				}

				timer -= Time.deltaTime;
				gameObject.transform.position =
					Vector2.MoveTowards(transform.position, oldPosition, returnSpeed * Time.deltaTime);

				yield return new WaitForEndOfFrame();
			}

			/*LeanTween.value(gameObject, 0f, force, duration / 2)
			         .setEaseInQuad().setOnUpdate(val => { forceToApply = val; });
			lanternRb.linearVelocity = Vector2.zero;
			while (LeanTween.isTweening(gameObject)) {
				lanternRb.linearVelocity = -direction * forceToApply;
				yield return new WaitForEndOfFrame();
			}*/

			yield return null;
		}

		/*private IEnumerator EnemyExtraForce(float force, Vector2 direction, float duration) {
			lanternRb.linearVelocity = Vector2.zero;
			while (duration > 0) {
				duration -= Time.deltaTime;

				lanternRb.linearVelocity = force * direction;

				yield return null;
			}

			lanternRb.linearVelocity = Vector2.zero;
			yield return null;
		}*/

		private void StateChanger() {
			CurrentEnemyState = isAwakened ? EnemyStates.AwakenedIdle : EnemyStates.Idle;
			if (isShooting) CurrentEnemyState        = EnemyStates.Shooting;
			if (IsKnockbackActive) CurrentEnemyState = EnemyStates.Knockback;
		}

		private bool PlayerDetected() {
			if (!playerTransform || !transform) return false; // idk wierd edge case thing
			if (!(Vector2.Distance(transform.position, playerTransform.position) < detectionRange)) return false;
			var hit = Physics2D.Linecast(transform.position, playerTransform.position,
			                             LayerMask.GetMask("Ground", "Player"));
			isAwakened = true;

			return hit.collider && hit.collider.gameObject.layer == LayerMask.NameToLayer("Player");
		}

		#endregion

		private void OnDrawGizmos() {
			if (playerTransform == null) return;
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(playerTransform.position, fleeRadius);

			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(playerTransform.position, followRadius);
		}
	}
}