using System.Collections;
using Enemy;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player {
	public class PlayerCombat : MonoBehaviour {
		public static PlayerCombat Instance;

		#region Fields

		[SerializeField] private Transform mouseCircle;

		[Header("Slash Settings")]
		[SerializeField] private float slashRadius;
		[SerializeField] private float slashDistance;
		[SerializeField] private float slashCooldown;

		[Header("Slash Recoil/Knockback Settings")]
		[SerializeField] private float slashRecoilForce;
		[SerializeField] private float slashRecoilDuration;
		[SerializeField] private float slashKnockbackForce;
		[SerializeField] private float slashKnockbackLength;
		[SerializeField] private int   slashDamage;

		[Header("Throw Settings")]
		[SerializeField] private float throwChargeSpeed;
		[SerializeField] private float throwMinForce;
		[SerializeField] private float throwMaxForce;
		[SerializeField] private float throwCooldown;
		[SerializeField] private float throwInputActivationAmount;
		[SerializeField] private float inAirTimeSlowLength;

		[Header("Throw Recoil Settings")]
		[SerializeField] private float throwRecoilForce;
		[SerializeField] private float throwRecoilLength;

		[Header("Parry Settings")]
		[SerializeField] private float parryLength;
		[SerializeField] private float parryRadius;
		[SerializeField] private float parryCooldown;
		[SerializeField] private float parryRecoilForce;
		[SerializeField] private float parryRecoilDuration;


		[Header("Drag and Drop")]
		[SerializeField] private GameObject slashVFX;
		[SerializeField] private GameObject throwObject;
		[SerializeField] private Camera     mainCamera;

		//? Private floats
		private float slashTimer;
		private float parryTimer;
		private float throwTimer;
		private float throwHeldInputTimer;

		//? Getters and setters
		public bool IsParrying { get; private set; }

		//? Private bools
		private bool throwInputHeld;
		private bool throwReady;

		//? Private vectors
		private Vector3 mousePositionInput;
		private Vector3 mouseVector;

		//? Private layers
		private LayerMask enemyLayer;
		private LayerMask parriableLayer;

		//? Components
		private Rigidbody2D playerRb;
		private Rigidbody2D rangedObjectRb;

		//? Coroutines
		private Coroutine parryCoroutine;
		private Coroutine throwAttackCoroutine;

		#endregion

		#region Unity Functions

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
			mouseVector                    = mainCamera.ScreenToWorldPoint(mousePositionInput);
			mouseCircle.transform.position = mouseVector;
			UpdateTimers();
			InputHeldChecker();
			ThrowAttack();
		}

		#endregion

		#region Functions

		private void UpdateTimers() {
			slashTimer -= Time.deltaTime;
			parryTimer -= Time.deltaTime;
			throwTimer -= Time.deltaTime;
		}

		private void InputHeldChecker() {
			if (throwInputHeld) {
				if (throwHeldInputTimer < throwInputActivationAmount) {
					throwHeldInputTimer += Time.unscaledDeltaTime;
				}

				if (throwHeldInputTimer >= throwInputActivationAmount) {
					throwReady = true;
				}
			} else {
				throwHeldInputTimer = 0f;
				throwReady          = false;
			}
		}

		#endregion

		#region Attack

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
				StartCoroutine(Lib.Combat.PreformedKnockback(enemy.GetComponent<Rigidbody2D>(), enemy.gameObject,
				                                             knockbackDirection,
				                                             slashKnockbackForce, slashKnockbackLength));
				StartCoroutine(Lib.Combat.TimeStop(0.05f));
			}
		}

		private void ThrowAttack() {
			if (!(throwTimer <= 0f) || !throwReady) return;
			throwAttackCoroutine ??= StartCoroutine(ThrowAttackIEnumerator());
		}

		private IEnumerator ThrowAttackIEnumerator() {
			var forceToApply       = throwMinForce;
			var inAriTimeSlowTimer = inAirTimeSlowLength;
			var stopTimeScaling    = false;

			while (throwInputHeld) {
				inAriTimeSlowTimer -= Time.unscaledDeltaTime;
				if (Time.timeScale > 0.1 && !stopTimeScaling) {
					Time.timeScale -= Time.unscaledDeltaTime * 10;
				} else if (inAriTimeSlowTimer <= 0f) {
					stopTimeScaling = true;
				}

				if (stopTimeScaling && Time.timeScale < 1) {
					Time.timeScale += Time.unscaledDeltaTime * 5;
				}

				if (forceToApply < throwMaxForce) {
					forceToApply += throwChargeSpeed;
				}

				yield return null;
			}

			Time.timeScale = 1f;

			var instantiatedObject   = Instantiate(throwObject, transform.position, Quaternion.identity);
			var instantiatedObjectRb = instantiatedObject.GetComponent<Rigidbody2D>();
			var direction            = (mouseVector - transform.position).normalized;

			instantiatedObjectRb.AddForce(forceToApply * direction,
			                              ForceMode2D.Impulse);
			StartCoroutine(ExtraForce(throwRecoilForce, direction * -1, throwRecoilLength));
			throwTimer = throwCooldown;

			Debug.Log(forceToApply);
			throwAttackCoroutine = null;
			yield return null;
		}

		#endregion


		#region Coroutines

		private IEnumerator ExtraForce(float force, Vector2 direction, float duration) {
			playerRb.linearVelocity = Vector2.zero;
			while (duration > 0) {
				duration -= Time.deltaTime;

				PlayerController.Instance.ExtraForce = force * direction;

				yield return null;
			}

			PlayerController.Instance.ExtraForce = Vector2.zero;
			yield return null;
		}

		private IEnumerator Parry() {
			var parryHit         = false;
			var parryLengthTimer = parryLength;

			IsParrying = true;

			while (parryLengthTimer > 0f) {
				parryLengthTimer -= Time.deltaTime;
				var parriedColliders = Physics2D.OverlapCircleAll(transform.position, parryRadius, parriableLayer);
				if (parriedColliders.Length > 0) {
					StartCoroutine(Lib.Combat.TimeStop(0.05f));
					parryHit         = true;
					parryLengthTimer = 0f;
					foreach (var parriedCollider in parriedColliders) {
						Destroy(parriedCollider.gameObject);
					}
				}

				yield return null;
			}

			IsParrying = false;
			if (!PlayerController.Instance.GetIsGrounded() && parryHit)
				StartCoroutine(ExtraForce(parryRecoilForce, Vector2.up, parryRecoilDuration));

			yield return null;
		}

		#endregion

		#region Input Callback

		private void OnSlash(InputValue value) {
			if (!value.isPressed || !(slashTimer <= 0f)) return;

			slashTimer = slashCooldown;
			SlashAttack();
		}

		private void OnParry(InputValue value) {
			if (!value.isPressed || !(parryTimer <= 0f) || throwInputHeld) return;
			parryCoroutine = StartCoroutine(Parry());
			parryTimer     = parryCooldown;
		}

		private void OnThrow(InputValue value) {
			throwInputHeld = value.isPressed;
		}

		private void OnMousePosition(InputValue value) {
			mousePositionInput = value.Get<Vector2>();
		}


		private void OnDrawGizmos() {
			Gizmos.color = Color.red;
			if (!PlayerController.Instance) return;
			var slashPosition =
				new Vector2(PlayerController.Instance.IsLookingRight ? 1 : -1 * slashDistance + transform.position.x,
				            transform.position.y);
			Gizmos.DrawWireSphere(slashPosition, slashRadius);


			Gizmos.DrawLine(transform.position, mouseVector);
		}

		#endregion
	}
}