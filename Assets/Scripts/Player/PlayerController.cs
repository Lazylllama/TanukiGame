using System.Collections;
using Logic;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player {
	public class PlayerController : MonoBehaviour {
		public static PlayerController Instance;

		#region Fields

		[Header("Movement settings")]
		[SerializeField] private float moveSpeed;
		[SerializeField] private float jumpForce;
		[SerializeField] private float normalGravity;
		[SerializeField] private float coyoteTime;
		[SerializeField] private float jumpBufferTime;

		[Header("Dash settings")]
		[SerializeField] private float dashForce;
		[SerializeField] private float dashCooldown;
		[SerializeField] private float dashLength;

		[Header("Misc settings")]
		[SerializeField] private float groundCheckDistance;

		//? Getters and setters
		[field: SerializeField] public MovingStates MovingState     { get; set; }
		public                         bool         IsLookingRight  { get; private set; }
		public                         bool         GetIsGrounded() => IsGrounded();
		[field: SerializeField] public bool         KnockbackActive { get;         set; }
		public                         Vector2      ExtraForce      { private get; set; }
		private                        Rigidbody2D  PlayerRb        { get;         set; }

		//? Private floats
		private                  float dashTimer;
		private                  float coyoteTimer;
		[SerializeField] private float jumpBufferTimer;
		private                  float fallSpeedDampingChangeThreshold;

		//? Private ints

		//? Private bools
		private bool jumpPressed;
		private bool dashPressed;
		private bool dashActive;

		//? Components
		private CapsuleCollider2D  playerCollider;
		private CameraFollowObject cameraFollowObject;

		//? Private vectors
		private Vector2 moveVector;

		//? Private coroutines
		private Coroutine dashCoroutine;

		//? Enums
		public enum MovingStates {
			Idle,
			Moving,
			Jumping,
			Falling,
			Dashing,
			Parrying,
			Knockback
		}

		#endregion

		#region Unity Functions

		private void Awake() {
			if (Instance != null) {
				Destroy(this.gameObject);
			} else {
				Instance = this;
			}

			PlayerRb           = GetComponent<Rigidbody2D>();
			playerCollider     = GetComponent<CapsuleCollider2D>();
			cameraFollowObject = FindAnyObjectByType<CameraFollowObject>();
		}

		private void Start() {
			fallSpeedDampingChangeThreshold = CameraManager.Instance.fallSpeedDampingChangeThreshold;
		}

		private void Update() {
			StateChanger();
			HandleCooldowns();

			if (PlayerRb.linearVelocityY < fallSpeedDampingChangeThreshold) {
				CameraManager.Instance.LerpYDamping(true);
			} else if (PlayerRb.linearVelocityY >= 0f) {
				CameraManager.Instance.LerpYDamping(false);
			}
		}

		private void FixedUpdate() {
			CheckSpriteFlip();
			MovementHandler();
		}

		#endregion

		private void MovementHandler() {
			if (dashActive) return;

			if (MovingState == MovingStates.Knockback) return;
			PlayerRb.linearVelocityX = moveVector.x * moveSpeed + ExtraForce.x;
			if (ExtraForce.y != 0) PlayerRb.linearVelocityY = ExtraForce.y;

			if (!(jumpBufferTimer >= 0) || !(coyoteTimer > 0)) return;
			coyoteTimer              = 0f;
			jumpBufferTimer          = 0f;
			jumpPressed              = false;
			PlayerRb.linearVelocityY = jumpForce;
		}


		private IEnumerator Dash() {
			dashPressed = false;
			dashActive  = true;
			var currentFacingDirection = IsLookingRight ? 1 : -1;
			var dashLengthTimer        = dashLength;
			StartCoroutine(Lib.Combat.TimeStop(0.02f));

			while (dashLengthTimer > 0) {
				dashLengthTimer -= Time.deltaTime;

				PlayerRb.linearVelocityX = dashForce * currentFacingDirection;
				PlayerRb.linearVelocityY = 0f;
				yield return null;
			}

			StartCoroutine(Lib.Combat.TimeStop(0.02f));
			PlayerRb.gravityScale = normalGravity;
			dashActive            = false;
			dashCoroutine         = null;
		}

		private void CheckSpriteFlip() {
			var oldDirection = IsLookingRight;
			IsLookingRight = moveVector.x switch {
				> 0 => true,
				< 0 => false,
				_   => IsLookingRight
			};

			if (oldDirection == IsLookingRight) return;
			cameraFollowObject.CallTurn();

			var rotator = new Vector3(transform.rotation.x, IsLookingRight ? 0f : 180f, transform.rotation.z);
			transform.rotation = Quaternion.Euler(rotator);
		}

		private void HandleCooldowns() {
			if (IsGrounded()) {
				coyoteTimer = coyoteTime;
			} else {
				coyoteTimer -= Time.deltaTime;
			}

			if (jumpPressed) {
				jumpBufferTimer = jumpBufferTime;
				jumpPressed     = false;
			} else {
				jumpBufferTimer -= Time.deltaTime;
			}

			dashTimer -= Time.deltaTime;
		}

		private bool IsGrounded() {
			var boxSize   = new Vector2(playerCollider.size.x * 0.5f, playerCollider.size.y * 0.5f);
			var boxCenter = (Vector2)transform.position + new Vector2(0, -playerCollider.size.y / 2);
			var hit = Physics2D.BoxCast(boxCenter, boxSize, 0, Vector2.down,
			                            groundCheckDistance, LayerMask.GetMask("Ground"));
			return hit.collider;
		}

		private void StateChanger() {
			if (!KnockbackActive) {
				if (PlayerRb.linearVelocity.x == 0f && IsGrounded()) MovingState  = MovingStates.Idle;
				if (PlayerRb.linearVelocity.x != 0f && IsGrounded()) MovingState  = MovingStates.Moving;
				if (PlayerRb.linearVelocity.y > 0f  && !IsGrounded()) MovingState = MovingStates.Jumping;
				if (PlayerRb.linearVelocity.y < 0f  && !IsGrounded()) MovingState = MovingStates.Falling;
				if (dashActive) MovingState                                       = MovingStates.Dashing;
				if (PlayerCombat.Instance.IsParrying) MovingState                 = MovingStates.Parrying;
			} else MovingState = MovingStates.Knockback;
		}

		#region Input Callbacks

		private void OnMove(InputValue value) {
			moveVector = value.Get<Vector2>();
			moveVector.Normalize();
		}

		private void OnJump(InputValue value) {
			jumpPressed = value.isPressed;
		}

		private void OnDash(InputValue value) {
			dashPressed = value.isPressed;
			if (!dashPressed || dashTimer > 0 || dashCoroutine != null) return;
			dashTimer     = dashCooldown;
			dashCoroutine = StartCoroutine(Dash());
		}

		#endregion
	}
}