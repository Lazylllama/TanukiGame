using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Effects {
	public class CardTilt : MonoBehaviour {
		private static readonly int Aspect = Shader.PropertyToID("_Aspect");
		private static readonly int Tilt   = Shader.PropertyToID("_Tilt");

		[SerializeField] private float       burnDuration = 1.0f;
		[SerializeField] private float       tiltAmount   = 8.0f;
		[SerializeField] private float       hoverScale   = 1.08f;
		[SerializeField] private Camera      cardCamera;
		[SerializeField] private float       flipAngle;
		[SerializeField] private GameObject  backImage, icon;
		[SerializeField] private CanvasGroup contentGroup;
		[SerializeField] private float       flipDuration = 0.25f, tiltSpeedOnCard = 5.0f, tiltSpeedOffCard = 2.0f;

		private RectTransform rectTransform, iconRectTransform;
		private Material      mat,   iconMat;
		private Image         image, iconImage;
		private Vector2 currentMouse;
		private Vector3 startScale;
		private Canvas  canvas;
		private bool    firstFrame = true;
		
		private static readonly int           BurnId     = Shader.PropertyToID("_Burn");
		
		
		private void Awake() {
			rectTransform = GetComponent<RectTransform>();
			image = GetComponent<Image>();
			
			iconRectTransform = icon.GetComponent<RectTransform>();
			iconImage = icon.GetComponent<Image>();
		}
		private void Start() {
			startScale                     = transform.localScale;
			canvas                         = GetComponentInParent<Canvas>().rootCanvas;
		}

		private void OnEnable() {
			RefreshMaterial();
			Flip();
		}

		private void RefreshMaterial() {
			var fresh = new Material(image.material);
			if (mat != null) Destroy(mat);
			mat            = fresh;
			image.material = mat;
			
			mat.SetFloat(Aspect, rectTransform.rect.width / rectTransform.rect.height);
			mat.SetFloat(BurnId, 0f);
			
			var imageFresh = new Material(iconImage.material);
			if (iconMat != null) Destroy(iconMat);
			iconMat = imageFresh;
			iconImage.material = iconMat;
			
			iconMat.SetFloat(Aspect, iconRectTransform.rect.width / iconRectTransform.rect.height);
			iconMat.SetFloat(BurnId, 0f);
			
			contentGroup.alpha = 1f;
		}


	
		public void Burn(Action onDone) {
			LeanTween.value(gameObject, 0f, 1f, burnDuration)
			         .setOnUpdate(e => mat.SetFloat(BurnId, e))
			         .setOnComplete(onDone);
			LeanTween.alphaCanvas(contentGroup, 0f, burnDuration * 0.6f);
		}

		private void Flip() {
			LeanTween.cancel(gameObject);
			backImage.SetActive(true);

			LeanTween.value(gameObject, 0f, 90f, flipDuration).setOnUpdate(e => flipAngle = e).setOnComplete(() => {
							 backImage.SetActive(false);
							 LeanTween.value(gameObject, -90f, 0f, flipDuration).setOnUpdate(e => flipAngle = e);
						 });
		}

		private void OnDestroy() {
			Destroy(mat);
			Destroy(iconMat);
		}

		// Update is called once per frame
		private void Update() {
			var mousePos = Mouse.current.position.ReadValue();

			var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePos, cam,
			                                                        out var localPos);
			localPos.x /= rectTransform.sizeDelta.x / 2.0f;
			localPos.y /= rectTransform.sizeDelta.y / 2.0f;

			var isOnCard = (localPos is { x: <= 1.0f and >= -1.0f, y: <= 1.0f and >= -1.0f });
			var t        = 1.0f - Mathf.Exp((isOnCard ? tiltSpeedOnCard : tiltSpeedOffCard) * Time.deltaTime);

			if (firstFrame) {
				currentMouse = isOnCard ? localPos : Vector2.zero;
				firstFrame   = false;
			} else {
				currentMouse = isOnCard
					               ? Vector2.Lerp(currentMouse, localPos,     t)
					               : Vector2.Lerp(currentMouse, Vector2.zero, t);
				var rot = transform.rotation;
				rot.y              = -currentMouse.x * tiltAmount;
				rot.x              = currentMouse.y  * tiltAmount;
				transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y + flipAngle, 0.0f));
			}

			t = 1.0f - Mathf.Exp((-20.0f) * Time.deltaTime);

			transform.localScale = Vector3.Lerp(transform.localScale,
			                                    isOnCard
				                                    ? new Vector3(hoverScale * startScale.x, hoverScale * startScale.y,
				                                                  1.0f       * startScale.z)
				                                    : startScale, t);

			mat.SetVector(Tilt, currentMouse);
			iconMat.SetVector(Tilt, currentMouse);
		}
	}
}