using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Effects {
	public class CardTilt : MonoBehaviour {
		private static readonly int Aspect = Shader.PropertyToID("_Aspect");
		private static readonly int Tilt   = Shader.PropertyToID("_Tilt");

		[SerializeField] private float      tiltAmount = 8.0f;
		[SerializeField] private float      hoverScale = 1.08f;
		[SerializeField] private Camera     cardCamera;
		[SerializeField] private float      flipAngle;
		[SerializeField] private GameObject backImage;
		[SerializeField] private CanvasGroup contentGroup;

		private RectTransform rectTransform;
		private Material      mat;
		private Vector2       currentMouse;
		private Vector3       startScale;
		private Canvas        canvas;
		private bool          firstFrame = true;

		private void Start() {
			startScale                     = transform.localScale;
			rectTransform                  = GetComponent<RectTransform>();
			mat                            = new Material(GetComponent<Image>().material);
			canvas                         = GetComponentInParent<Canvas>().rootCanvas;
			GetComponent<Image>().material = mat;

			mat.SetFloat(Aspect, rectTransform.rect.width / rectTransform.rect.height);
		}

		private void OnEnable() {
			Flip();
		}

		
		[ContextMenu("Burn")]
		private void Burn() {
			LeanTween.value(gameObject, 0f, 1f, 1.0f).setOnUpdate(e => mat.SetFloat("_Burn", e));
			LeanTween.alphaCanvas(contentGroup, 0f, 0.6f);
		}

		private void Flip() {
			LeanTween.cancel(gameObject);
			backImage.SetActive(true);

			LeanTween.value(gameObject, 0f, 90f, 0.25f).setOnUpdate(e => flipAngle = e).setOnComplete(() => {
							 backImage.SetActive(false);
							 LeanTween.value(gameObject, -90f, 0f, 0.25f).setOnUpdate(e => flipAngle = e);
						 });
		}
		
		private void OnDestroy() => Destroy(mat);

		// Update is called once per frame
		private void Update() {
			var mousePos = Mouse.current.position.ReadValue();

			var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePos, cam,
			                                                        out var localPos);
			localPos.x /= rectTransform.sizeDelta.x / 2.0f;
			localPos.y /= rectTransform.sizeDelta.y / 2.0f;

			var isOnCard = (localPos is { x: <= 1.0f and >= -1.0f, y: <= 1.0f and >= -1.0f });
			var t        = 1.0f - Mathf.Exp((isOnCard ? -05.0f : -2.0f) * Time.deltaTime);

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
		}
	}
}