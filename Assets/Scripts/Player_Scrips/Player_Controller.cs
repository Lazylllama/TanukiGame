using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Controller : MonoBehaviour {
	public static Player_Controller Instance;
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


	// Getters and setters
	public MovingStates MovingState     { get;         private set; }
	public int          FacingDirection { get;         private set; }
	public Vector2      ExtraForce      { private get; set; }


	//Private floats
	private float dashTimer;

	//Private ints


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
		if (Instance != null) {
			Destroy(this.gameObject);
		} else {
			Instance = this;
		}

		FacingDirection = 1;

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
		if (ExtraForce.magnitude > 0) playerRb.linearVelocity = new Vector2(moveVector.x + ExtraForce.x, ExtraForce.y);
		else playerRb.linearVelocityX                         = moveVector.x * moveSpeed;

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

			//playerRb.linearVelocity = dashDirection * dashForce; //Vilket håll som helst
			playerRb.linearVelocityX = FacingDirection * dashForce; //Bara åt sidan
			yield return null;
		}

		dashActive    = false;
		dashCoroutine = null;
	}

	private void SpriteFlip() {
		FacingDirection = moveVector.x switch {
			> 0 => 1,
			< 0 => -1,
			_   => FacingDirection
		};
		transform.localScale = new Vector3(FacingDirection, 1f, 1f);
	}

	private bool IsGrounded() {
		var boxSize   = new Vector2(playerCollider.size.x, playerCollider.size.y * 0.5f);
		var boxCenter = (Vector2)transform.position + new Vector2(0, -playerCollider.size.y / 2);
		var hit = Physics2D.BoxCast(boxCenter, boxSize, 0, Vector2.down,
		                            groundCheckDistance, LayerMask.GetMask("Ground"));
		return hit.collider;
	}

	private void StateChanger() {
		if (playerRb.linearVelocity.x == 0f && IsGrounded()) MovingState  = MovingStates.Idle;
		if (playerRb.linearVelocity.x != 0f && IsGrounded()) MovingState  = MovingStates.Moving;
		if (playerRb.linearVelocity.y > 0f  && !IsGrounded()) MovingState = MovingStates.Jumping;
		if (playerRb.linearVelocity.y < 0f  && !IsGrounded()) MovingState = MovingStates.Falling;
		if (dashActive) MovingState                                       = MovingStates.Dashing;
	}

	public enum MovingStates {
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