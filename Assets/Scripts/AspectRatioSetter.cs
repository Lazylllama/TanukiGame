using UnityEngine;
using UnityEngine.UI;

public class AspectRatioSetter : MonoBehaviour
{
    void Start()
    {
	    var rt = GetComponent<RectTransform>();
        var img = GetComponent<RawImage>();
        var mat = new Material(img.material);
        img.material = mat;
        mat.SetFloat("_Aspect", rt.rect.width / rt.rect.height);
    }
}
