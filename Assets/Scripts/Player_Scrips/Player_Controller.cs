using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Controller : MonoBehaviour {
	[Header("Movement settings")]
	[SerializeField] private float moveSpeed;
	[SerializeField] private float jumpForce;

	[Header("Misc settings")]
	[SerializeField] private float groundCheckDistance;

	//Bools
	private bool jumpPressed;

	//Components
	private Rigidbody2D       playerRb;
	private CapsuleCollider2D playerCollider;

	//Vectors
	private Vector2 moveVector;

	private void Awake() {
		playerRb       = GetComponent<Rigidbody2D>();
		playerCollider = GetComponent<CapsuleCollider2D>();
	}

	private void FixedUpdate() {
		MovementHandler();
	}

	private void MovementHandler() {
		playerRb.linearVelocityX = moveVector.x * moveSpeed;

		if (!jumpPressed || !IsGrounded()) return;
		playerRb.linearVelocityY = jumpForce;
		jumpPressed              = false;
	}

	private bool IsGrounded() {
		var boxSize   = new Vector2(playerCollider.size.x, playerCollider.size.y * 0.5f);
		var boxCenter = (Vector2)transform.position + new Vector2(0, -playerCollider.size.y / 2);
		var hit = Physics2D.BoxCast(boxCenter, boxSize, 0, Vector2.down,
		                            groundCheckDistance, LayerMask.GetMask("Ground"));
		return hit.collider;
	}

	private void OnMove(InputValue value) {
		moveVector = value.Get<Vector2>();
		moveVector.Normalize();
	}

	private void OnJump(InputValue value) {
		jumpPressed = value.isPressed;
	}
}