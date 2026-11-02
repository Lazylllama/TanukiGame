using UnityEngine;
using UnityEngine.UI;

public class AspectRatioSetter : MonoBehaviour
{
	private Material mat;
	
    private void Start()
    {
	    var rt = GetComponent<RectTransform>();
        var img = GetComponent<RawImage>();
	    mat = new Material(img.material);
        img.material = mat;
        mat.SetFloat("_Aspect", rt.rect.width / rt.rect.height);
    }

    private void OnDestroy() {
	    Destroy(mat);
    }
}
