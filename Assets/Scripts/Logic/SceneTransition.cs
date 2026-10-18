using System;
using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Logic {
	public class SceneTransition : MonoBehaviour {
		[SerializeField] private RawImage transitionImage;
		[SerializeField] private float    coverTime = 1f;
		[SerializeField] private float    holdTime  = 0.5f;
		[SerializeField] private bool     enableTestKeys;

		public static SceneTransition Instance;

		private static readonly int ProgressPropId = Shader.PropertyToID("_Progress");
		private static readonly int ReversedPropId = Shader.PropertyToID("_Reversed");

		private Material mat;
		private bool     busy;

		private void Awake() {
			if (Instance != null) {
				Destroy(gameObject);
				return;
			}

			Instance = this;

			mat                      = new Material(transitionImage.material);
			transitionImage.material = mat;
			mat.SetFloat(ProgressPropId, 0f);

			DontDestroyOnLoad(gameObject);
		}

		private void OnDestroy() {
			if (mat) Destroy(mat);
		}

		private void Update() {
			if (!enableTestKeys) return;
			if (Keyboard.current.tKey.wasPressedThisFrame) TestTransition();
			if (Keyboard.current.rKey.wasPressedThisFrame) TestTransitionReverse();
		}

		public void TransitionToScene(string sceneName, Action onScreenCovered) {
			if (busy) return;
			StartCoroutine(TransitionCoroutine(sceneName, onScreenCovered));
		}

		private void TweenProgress(bool newState, float duration) {
			var target = newState ? 1 : 0;
			var from   = mat.GetFloat(ProgressPropId);
			if (Mathf.Approximately(from, target)) {
				busy = false;
				return;
			}

			LeanTween.cancel(gameObject);
			mat.SetFloat(ReversedPropId, newState ? 0 : 1);
			LeanTween.value(gameObject, from, target, duration)
			         .setEase(LeanTweenType.easeInOutSine)
			         .setOnUpdate(value => mat.SetFloat(ProgressPropId, value))
			         .setIgnoreTimeScale(true);
		}

		private IEnumerator TransitionCoroutine(string sceneName, Action onScreenCovered) {
			if (PlayerController.Instance) PlayerController.Instance.MovementDisabled = true;
			busy = true;

			TweenProgress(true, coverTime);
			yield return new WaitForSecondsRealtime(coverTime);

			var loadScene = SceneManager.LoadSceneAsync(sceneName);

			if (loadScene != null) {
				loadScene.allowSceneActivation = false;

				yield return new WaitForSecondsRealtime(holdTime);

				while (loadScene.progress < 0.9f) yield return null;

				onScreenCovered.Invoke();

				loadScene.allowSceneActivation = true;
				yield return loadScene;
			}

			TweenProgress(false, coverTime);
			yield return new WaitForSecondsRealtime(coverTime);

			busy = false;
			if (PlayerController.Instance) PlayerController.Instance.MovementDisabled = false;
		}

		private void TestTransition()        => TransitionToScene("PreGameScene", () => { });
		private void TestTransitionReverse() => TweenProgress(false, coverTime);
	}
}