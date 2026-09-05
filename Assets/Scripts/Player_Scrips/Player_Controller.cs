using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Controller : MonoBehaviour {
	[Header("Movement settings")]
	[SerializeField] private float moveSpeed;
	[SerializeField] private float jumpForce;

	[Header("Dash settings")]
	[SerializeField] private float dashForce;
	[SerializeField] private float dashCooldown;
	[SerializeField] private float dashLength;

	[Header("Misc settings")]
	[SerializeField] private float groundCheckDistance;
	[SerializeField] private Camera mainCamera;


	[SerializeField] private MovingStates movingStates;

	//Private floats
	private float dashTimer;

	//Private ints
	private int facingDirection = 1;

	//Private bools
	private bool jumpPressed;
	private bool dashPressed;
	private bool dashActive;

	//Components
	private Rigidbody2D       playerRb;
	private CapsuleCollider2D playerCollider;

	//Private vectors
	private Vector2 moveVector;
	private Vector2 mousePosition;
	private Vector2 mousePositionInput;

	//Private coroutines
	private Coroutine dashCoroutine;

	private void Awake() {
		playerRb       = GetComponent<Rigidbody2D>();
		playerCollider = GetComponent<CapsuleCollider2D>();
	}

	private void Update() {
		SpriteFlip();
		StateChanger();
		mousePosition =  mainCamera.ScreenToWorldPoint(mousePositionInput);
		dashTimer     -= Time.deltaTime;
	}

	private void FixedUpdate() {
		MovementHandler();
	}

	private void MovementHandler() {
		if (dashActive) return;
		playerRb.linearVelocityX = moveVector.x * moveSpeed;

		if (!jumpPressed || !IsGrounded()) return;
		playerRb.linearVelocityY = jumpForce;
		jumpPressed              = false;
	}


	private IEnumerator Dash() {
		dashPressed = false;
		dashActive  = true;
		var dashLengthTimer = dashLength;
		var dashDirection   = mousePosition.normalized;

		while (dashLengthTimer > 0) {
			dashLengthTimer -= Time.deltaTime;

			playerRb.linearVelocity = dashDirection * dashForce;
			yield return null;
		}

		dashActive    = false;
		dashCoroutine = null;
	}

	private void SpriteFlip() {
		facingDirection = playerRb.linearVelocity.x switch {
			> 0 => 1,
			< 0 => -1,
			_   => facingDirection
		};
		transform.localScale = new Vector3(facingDirection, 1f, 1f);
	}

	private bool IsGrounded() {
		var boxSize   = new Vector2(playerCollider.size.x, playerCollider.size.y * 0.5f);
		var boxCenter = (Vector2)transform.position + new Vector2(0, -playerCollider.size.y / 2);
		var hit = Physics2D.BoxCast(boxCenter, boxSize, 0, Vector2.down,
		                            groundCheckDistance, LayerMask.GetMask("Ground"));
		return hit.collider;
	}

	private void StateChanger() {
		if (playerRb.linearVelocity.x == 0f && IsGrounded()) movingStates  = MovingStates.Idle;
		if (playerRb.linearVelocity.x != 0f && IsGrounded()) movingStates  = MovingStates.Moving;
		if (playerRb.linearVelocity.y > 0f  && !IsGrounded()) movingStates = MovingStates.Jumping;
		if (playerRb.linearVelocity.y < 0f  && !IsGrounded()) movingStates = MovingStates.Falling;
		if (dashActive) movingStates                                       = MovingStates.Dashing;
	}

	private enum MovingStates {
		Idle,
		Moving,
		Jumping,
		Falling,
		Dashing
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

	private void OnMousePosition(InputValue value) {
		mousePositionInput = value.Get<Vector2>();
	}
}