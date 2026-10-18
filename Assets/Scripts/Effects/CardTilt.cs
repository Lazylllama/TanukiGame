using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CardTilt : MonoBehaviour {
	[SerializeField] private float      tiltAmount = 8.0f;
	[SerializeField] private float      hoverScale = 1.08f;
	[SerializeField] private Camera     cardCamera;
	[SerializeField] private float      flipAngle;
	[SerializeField] private GameObject backImage;
	
	private                  RectTransform rectTransform;
	private                  Material      mat;
	private                  Vector2       currentMouse;
	private                  bool          firstFrame = true;
	private                  Vector3       startScale;
	
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
	    startScale = transform.localScale;
	    rectTransform = GetComponent<RectTransform>();
        mat = new Material(GetComponent<Image>().material);
        GetComponent<Image>().material = mat;
        
        mat.SetFloat("_Aspect", rectTransform.rect.width / rectTransform.rect.height);

        
    }

    private void OnEnable() {
	    Flip();
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
    void Update()
    {
	    var mousePos = Mouse.current.position.ReadValue();
	    RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePos, cardCamera, out var localPos);
	    localPos.x /= rectTransform.sizeDelta.x / 2.0f;
	    localPos.y /= rectTransform.sizeDelta.y / 2.0f;
	    
	    var isOnCard = (localPos is { x: <= 1.0f and >= -1.0f, y: <= 1.0f and >= -1.0f });
	    var t        = 1.0f - Mathf.Exp((isOnCard ? -05.0f : -2.0f) * Time.deltaTime);
	    
	    if (firstFrame) {
		    currentMouse = isOnCard ? localPos : Vector2.zero;
		    firstFrame = false;
	    } else {
		    currentMouse = isOnCard ? Vector2.Lerp(currentMouse, localPos, t) : Vector2.Lerp(currentMouse, Vector2.zero, t);
		    var rot = transform.rotation;
		    rot.y              = -currentMouse.x * tiltAmount;
		    rot.x              = currentMouse.y * tiltAmount;
		    transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y + flipAngle, 0.0f));
	    }
	    
	    t = 1.0f - Mathf.Exp((-20.0f) * Time.deltaTime);
	    
	    transform.localScale = Vector3.Lerp(transform.localScale, isOnCard ? new Vector3(hoverScale * startScale.x, hoverScale * startScale.y, 1.0f * startScale.z) : startScale, t);

	    mat.SetVector("_Tilt", currentMouse);
    }
}
