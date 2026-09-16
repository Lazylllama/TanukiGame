using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemy {
	public class LanternEnemy : MonoBehaviour {
		[Header("Lantern Settings")]
		[SerializeField] private float attackSpeed;
		[SerializeField] private float attackCooldown;

		[Header("Hover Settings")]
		[SerializeField] private float hoverSpeed;
		[SerializeField] private float hoverHeight;
		[SerializeField] private float returnSpeed;
		[SerializeField] private float positionChangeFrequency;
		[SerializeField] private float hoverPositionAmountX;
		[SerializeField] private float hoverPositionAmountY;
		
		[Header("Awakening Settings")]
		[SerializeField] private float awakeningSpeed;


		[Header("Recoil Settings")]
		[SerializeField] private float recoilForce;
		[SerializeField] private float recoilLength;

		//Private floats
		private float attackTimer;
		private float positionChangeTimer;

		//Private bools
		private bool isInvulnerable;


		//Private vectors
		private Vector3 currentPosition;
		private Vector3 targetPosition;
		private Vector3 previousPosition;

		//Component references
		private Rigidbody2D lanternRb;

		private void Awake() {
			lanternRb = GetComponent<Rigidbody2D>();
		}

		private void Start() {
			isInvulnerable = true;
		}

		private void Update() {
			Hover();
		}

		private void Hover() {
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
		}

		private IEnumerator LanternAwakening() {
			var wantedPosition = new Vector3(transform.position.x, transform.position.y + hoverHeight, transform.position.z);
			var positionOffset  = hoverHeight;

			while (positionOffset >= 0f) {
				transform.position = Vector3.MoveTowards(transform.position, wantedPosition, awakeningSpeed * Time.deltaTime);
			}

			isInvulnerable = false;
			yield return null;
		}
	}
}