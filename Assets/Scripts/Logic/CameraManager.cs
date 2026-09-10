using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Logic {
	public class CameraManager : MonoBehaviour {
		public static CameraManager Instance;

		[Header("Fall/jump dampening")]
		[SerializeField] private float fallPanAmount = 0.25f;
		[SerializeField] private float fallPanTime = 0.35f;

		public float fallSpeedDampeningChangeThreshold = -15f;

		private CinemachineCamera           currentCinemachineCamera;
		private CinemachineCamera[]         allCinemachineCameras;
		private CinemachinePositionComposer currentPositionComposer;

		private float normPanYAmount;

		private void Awake() {
			if (Instance == null) {
				Instance = this;
			} else {
				Destroy(this);
			}

			allCinemachineCameras = GetComponentsInChildren<CinemachineCamera>();

			foreach (var camera in allCinemachineCameras) {
				if (!camera.enabled) continue;

				currentCinemachineCamera = camera;
				currentPositionComposer  = currentCinemachineCamera.GetComponent<CinemachinePositionComposer>();
				break;
			}

			normPanYAmount = currentPositionComposer.Damping.y;
		}

		public void LerpYDamping(bool isPlayerFalling) {
			var endDampAmount = isPlayerFalling ? fallPanAmount : normPanYAmount;

			LeanTween.cancel(gameObject);
			LeanTween.value(gameObject, currentPositionComposer.Damping.y, endDampAmount, fallPanTime)
			         .setOnUpdate((float value) => {
				                      var damping = currentPositionComposer.Damping;
				                      damping.y                       = value;
				                      currentPositionComposer.Damping = damping;
			                      });
		}
	}
}