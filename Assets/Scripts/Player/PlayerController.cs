using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player {
}

public class PlayerController : MonoBehaviour {
	public static PlayerController Instance;
	[Header("Movement settings")]
	[SerializeField] private float moveSpeed;
	[SerializeField] private float jumpForce;
	[SerializeField] private float normalGravity;
	[SerializeField] private float coyoteTime;

	[Header("Dash settings")]
	[SerializeField] private float dashForce;
	[SerializeField] private float dashCooldown;
	[SerializeField] private float dashLength;

	[Header("Misc settings")]
	[SerializeField] private float groundCheckDistance;


	// Getters and setters
	[field: SerializeField] public MovingStates MovingState    { get;         private set; }
	public                         bool         IsLookingRight { get;         private set; }
	public                         Vector2      ExtraForce     { private get; set; }
	private                        Rigidbody2D  PlayerRb       { get;         set; }


	//Private floats
	private float dashTimer;
	private float coyoteTimer;

	//Private ints

	//Private bools
	private bool jumpPressed;
	private bool dashPressed;
	private bool dashActive;

	//Components
	private CapsuleCollider2D  playerCollider;
	private CameraFollowObject cameraFollowObject;

	//Private vectors
	private Vector2 moveVector;

	//Private coroutines
	private Coroutine dashCoroutine;

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

	private void Update() {
		StateChanger();
		HandleCooldowns();
	}

	private void FixedUpdate() {
		SpriteFlip();
		MovementHandler();
	}

	private void MovementHandler() {
		if (dashActive) return;

		PlayerRb.linearVelocityX = moveVector.x * moveSpeed + ExtraForce.x;
		if (ExtraForce.y != 0) PlayerRb.linearVelocityY = ExtraForce.y;

		if (!jumpPressed || !(coyoteTimer > 0)) return;
		coyoteTimer              = 0f;
		jumpPressed              = false;
		PlayerRb.linearVelocityY = jumpForce;
	}


	private IEnumerator Dash() {
		dashPressed = false;
		dashActive  = true;
		var currentFacingDirection = IsLookingRight ? 1 : -1;
		var dashLengthTimer        = dashLength;

		while (dashLengthTimer > 0) {
			dashLengthTimer -= Time.deltaTime;

			PlayerRb.linearVelocityX = dashForce * currentFacingDirection;
			PlayerRb.linearVelocityY = 0f;
			yield return null;
		}

		PlayerRb.gravityScale = normalGravity;
		dashActive            = false;
		dashCoroutine         = null;
	}

	private void SpriteFlip() {
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
		if (PlayerRb.linearVelocity.x == 0f && IsGrounded()) MovingState  = MovingStates.Idle;
		if (PlayerRb.linearVelocity.x != 0f && IsGrounded()) MovingState  = MovingStates.Moving;
		if (PlayerRb.linearVelocity.y > 0f  && !IsGrounded()) MovingState = MovingStates.Jumping;
		if (PlayerRb.linearVelocity.y < 0f  && !IsGrounded()) MovingState = MovingStates.Falling;
		if (dashActive) MovingState                                       = MovingStates.Dashing;
		if (PlayerCombat.Instance.isParrying) MovingState                 = MovingStates.Parrying;
	}

	public enum MovingStates {
		Idle,
		Moving,
		Jumping,
		Falling,
		Dashing,
		Parrying
	}

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
}