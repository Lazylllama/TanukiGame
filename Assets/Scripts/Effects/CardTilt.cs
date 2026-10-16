using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CardTilt : MonoBehaviour {
	[SerializeField] private float tiltAmount = 8.0f;
	
	private RectTransform rectTransform;
	private Material mat;
	private Vector2  currentMouse;
	private bool     firstFrame = true;
	
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
	    rectTransform = GetComponent<RectTransform>();
        mat = new Material(GetComponent<Image>().material);
        GetComponent<Image>().material = mat;
    }

    // Update is called once per frame
    void Update()
    {
	    var mousePos = Mouse.current.position.ReadValue();
	    RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePos, Camera.main, out var localPos);
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
		    transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y, 0.0f));
	    }

	    mat.SetVector("_Tilt", currentMouse);
	    Debug.Log(localPos + " " + currentMouse + " " + t + " "  + isOnCard);
    }
}
