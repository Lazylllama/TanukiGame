using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Logic {
	public class LoadingScreenManager : MonoBehaviour {
		[SerializeField] private Image       logoFillImage;
		[SerializeField] private float       tweenDuration = 1.5f;
		[SerializeField] private VideoPlayer videoPlayer;
		
		private void Start() {
			StartCoroutine(StartLoading());
		}

		private void SetLogoFillAmount(float amount) {
			LeanTween.value(gameObject, logoFillImage.fillAmount, amount, tweenDuration)
			         .setEase(amount >= 1 ? LeanTweenType.easeOutCubic : LeanTweenType.easeSpring)
			         .setOnUpdate(val => { logoFillImage.fillAmount = val; });
		}

		private IEnumerator StartLoading() {
			var op = SceneManager.LoadSceneAsync($"GameScene");
			op.allowSceneActivation = false;

			while (op is { progress: < 0.9f }) {
				SetLogoFillAmount(op.progress);
				yield return new WaitForSeconds(2f);
			}

			SetLogoFillAmount(1f);
			yield return new WaitForSeconds(tweenDuration);
			yield return new WaitForSeconds((float)(Time.timeSinceLevelLoad < videoPlayer.clip.length
				                                        ? videoPlayer.clip.length - Time.timeSinceLevelLoad
				                                        : 0f));
			op.allowSceneActivation = true;
		}
	}
}