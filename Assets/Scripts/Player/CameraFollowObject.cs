using UnityEngine;

namespace Player {
	public class CameraFollowObject : MonoBehaviour {
		[SerializeField] private float flipYRotationTime = 0.5f;

		private static bool IsLookingRight => PlayerController.Instance && PlayerController.Instance.IsLookingRight;

		private void Update() {
			transform.position = PlayerController.Instance.transform.position;
		}

		public void CallTurn() {
			LeanTween.rotateY(gameObject, IsLookingRight ? 0f : 180f, flipYRotationTime).setEaseOutSine();
		}
	}
}