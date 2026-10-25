using System;
using Player;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

namespace Logic {
	public class CameraManager : MonoBehaviour {
		#region Fields

		public static CameraManager Instance;

		[Header("Fall/jump dampening")]
		[SerializeField] private float fallPanAmount = 0.25f;
		[SerializeField] private float fallPanTime = 0.35f;

		[Header("Effects")]
		[SerializeField] private float hurtPulseDuration = 0.5f;
		[SerializeField] private float throwSlowmoSpeed = 6f;

		public float fallSpeedDampingChangeThreshold = -15f;

		private CinemachineCamera           currentCinemachineCamera;
		private CinemachineCamera[]         allCinemachineCameras;
		private CinemachinePositionComposer currentPositionComposer;

		[SerializeField] private CinemachineImpulseSource recoilImpulseSource;
		[SerializeField] private CinemachineImpulseSource bounceImpulseSource;

		[SerializeField] private Volume hurtVolume;
		[SerializeField] private Volume throwSlowmoVolume;

		private float normPanYAmount;

		[Header("Data")]
		public bool isLerpingYDamping;
		public bool lerpedFromPlayerFall;

		#endregion

		#region Unity Functions

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

		private void Update() {
			var chargingThrow = PlayerCombat.Instance && PlayerCombat.Instance.IsChargingThrow;
			throwSlowmoVolume.weight = Mathf.MoveTowards(throwSlowmoVolume.weight, chargingThrow ? 1f : 0f,
			                                             Time.unscaledDeltaTime * throwSlowmoSpeed);
		}

		#endregion

		#region Functions

		public void LerpYDamping(bool isPlayerFalling) {
			if (isLerpingYDamping) return;
			if (lerpedFromPlayerFall == isPlayerFalling) return;

			var endDampAmount = isPlayerFalling ? fallPanAmount : normPanYAmount;
			isLerpingYDamping = true;

			LeanTween.cancel(gameObject);
			LeanTween.value(gameObject, currentPositionComposer.Damping.y, endDampAmount, fallPanTime)
			         .setOnUpdate((value) => {
				                      var damping = currentPositionComposer.Damping;
				                      damping.y                       = value;
				                      currentPositionComposer.Damping = damping;
			                      })
			         .setOnComplete(() => {
				                        isLerpingYDamping    = false;
				                        lerpedFromPlayerFall = isPlayerFalling;
			                        });
		}

		public enum CameraShakeType {
			Bounce,
			Recoil
		}

		public void ShakeCamera(float force, CameraShakeType type = CameraShakeType.Recoil) {
			switch (type) {
				case CameraShakeType.Bounce:
					bounceImpulseSource.GenerateImpulseWithForce(force);
					break;
				case CameraShakeType.Recoil:
					recoilImpulseSource.GenerateImpulseWithForce(force);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		public void HurtPulse() {
			LeanTween.cancel(hurtVolume.gameObject);
			LeanTween.value(hurtVolume.gameObject, 1f, 0f, hurtPulseDuration)
			         .setOnUpdate((value) => hurtVolume.weight = value)
			         .setEaseOutQuad();
		}

		#endregion
	}
}