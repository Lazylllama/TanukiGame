using System;
using UnityEngine;

namespace Player {
	public class CameraFollowObject : MonoBehaviour {
		[SerializeField] private float flipYRotationTime = 0.5f;

		private static bool Initialized    => PlayerController.Instance != null;
		private static bool IsLookingRight => PlayerController.Instance && PlayerController.Instance.IsLookingRight;

		private float pauseTimer;

		public void FlingCamera() {
			//? looks very good with transition
			transform.position += Vector3.right * 50f;
			pauseTimer         =  0.1f;
		}

		private void Update() {
			if (!Initialized) return;
			if (pauseTimer > 0) {
				pauseTimer -= Time.deltaTime;
				return;
			}

			transform.position = PlayerController.Instance.transform.position;
		}


		public void CallTurn() {
			LeanTween.rotateY(gameObject, IsLookingRight ? 0f : 180f, flipYRotationTime).setEaseOutSine();
		}
	}
}