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
		[SerializeField] private GameObject rangedObject;
		[SerializeField] private Camera     mainCamera;

		//? Private floats
		private                  float slashTimer;
		private                  float parryTimer;
		[SerializeField] private float rangedAttackTimer;

		//? Getters and setters
		public bool IsParrying { get; private set; }

		//? Private bools
		[SerializeField] private bool rangedAttackHeld;

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
		private Coroutine rangedAttackCoroutine;

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

		private void Update() => UpdateTimers();

		#endregion

		#region Functions

		private void UpdateTimers() {
			mouseVector                    =  mainCamera.ScreenToWorldPoint(mousePositionInput);
			mouseCircle.transform.position =  mouseVector;
			slashTimer                     -= Time.deltaTime;
			parryTimer                     -= Time.deltaTime;
			rangedAttackTimer              -= Time.deltaTime;
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
				StartCoroutine(Lib.Combat.PreformedKnockback(enemy.GetComponent<Rigidbody2D>(), knockbackDirection,
				                                             slashKnockbackForce, slashKnockbackLength));
				StartCoroutine(Lib.Combat.TimeStop(0.05f));
			}
		}

		private IEnumerator RangedAttackIEnumerator() {
			var forceToApply = rangedAttackMinForce;

			while (rangedAttackHeld) {
				Time.timeScale -= Time.deltaTime * 10;

				if (forceToApply < rangedAttackMaxForce) {
					forceToApply += rangedAttackChargeSpeed;
				}

				yield return null;
			}

			Time.timeScale = 1f;

			var instantiatedObject   = Instantiate(rangedObject, transform.position, Quaternion.identity);
			var instantiatedObjectRb = instantiatedObject.GetComponent<Rigidbody2D>();

			instantiatedObjectRb.AddForce(forceToApply * (mouseVector - transform.position).normalized,
			                              ForceMode2D.Impulse);
			rangedAttackTimer = rangedAttackCooldown;

			Debug.Log(forceToApply);
			rangedAttackCoroutine = null;
			yield return null;
		}

		#endregion


		#region Coroutines

		private static IEnumerator ExtraForce(float force, Vector2 direction, float duration) {
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

			IsParrying               = true;
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

			IsParrying = false;

			if (parryHit) StartCoroutine(ExtraForce(parryRecoilForce, Vector2.up, parryRecoilDuration));

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
			if (!value.isPressed || !(parryTimer <= 0f)) return;
			parryCoroutine = StartCoroutine(Parry());
			parryTimer     = parryCooldown;
		}

		private void OnRangedAttack(InputValue value) {
			rangedAttackHeld = value.isPressed;
			if (!value.isPressed) return;
			if (rangedAttackTimer <= 0f) {
				if (rangedAttackCoroutine == null) {
					rangedAttackCoroutine = StartCoroutine(RangedAttackIEnumerator());
				}
			}
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