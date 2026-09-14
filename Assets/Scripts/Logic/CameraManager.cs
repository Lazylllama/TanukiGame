using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Logic {
	public class CameraManager : MonoBehaviour {
		public static CameraManager Instance;

		[Header("Fall/jump dampening")]
		[SerializeField] private float fallPanAmount = 0.25f;
		[SerializeField] private float fallPanTime = 0.35f;

		public float fallSpeedDampingChangeThreshold = -15f;

		private CinemachineCamera           currentCinemachineCamera;
		private CinemachineCamera[]         allCinemachineCameras;
		private CinemachinePositionComposer currentPositionComposer;

		private float normPanYAmount;

		[Header("Data")]
		public bool isLerpingYDamping;
		public bool lerpedFromPlayerFall;

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
			if (isLerpingYDamping) {
				print("Already lerping to a new damping value");
				return;
			}

			if (lerpedFromPlayerFall == isPlayerFalling) {
				print("Already lerped to the same damping value");
				return;
			}

			var endDampAmount = isPlayerFalling ? fallPanAmount : normPanYAmount;
			isLerpingYDamping = true;

			print($"Lerping Y damping to {endDampAmount} because player is falling: {isPlayerFalling}");

			LeanTween.cancel(gameObject);
			LeanTween.value(gameObject, currentPositionComposer.Damping.y, endDampAmount, fallPanTime)
			         .setOnUpdate((value) => {
				                      var damping = currentPositionComposer.Damping;
				                      damping.y                       = value;
				                      currentPositionComposer.Damping = damping;
				                      print($"Set damping to {value}");
			                      })
			         .setOnComplete(() => {
				                        isLerpingYDamping    = false;
				                        lerpedFromPlayerFall = isPlayerFalling;
			                        });
		}
	}
}