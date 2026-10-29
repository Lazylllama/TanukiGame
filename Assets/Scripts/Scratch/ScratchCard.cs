using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ScratchCard : MonoBehaviour {
	private                  RenderTexture rt;
	private                  Material      mat;
	private                  RectTransform rectTransform;
	[SerializeField] private Canvas        canvas;
	[SerializeField] private Material      scratchBrushMaterial;
	
	private Vector2 lastMousePos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
	   
	    var image = GetComponent<RawImage>();
	    rectTransform = gameObject.GetComponent<RectTransform>();
        mat = new Material(image.material);
        image.material = mat;
        rt  = new RenderTexture(512, (int)(512f * rectTransform.rect.height / rectTransform.rect.width), 0, RenderTextureFormat.R8);
        rt.Create();
        var active = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = active;
        mat.SetTexture("_MaskTex", rt);
        scratchBrushMaterial.SetFloat("_Aspect", rectTransform.rect.width / rectTransform.rect.height);
    }

    // Update is called once per frame
    void Update()
    {
	    var mousePos = Mouse.current.position.ReadValue();

	    var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
	    RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePos, cam,
	                                                            out var localPos);
	    localPos.x /= rectTransform.rect.width;
	    localPos.y /= rectTransform.rect.height;
	    localPos += new Vector2(0.5f, 0.5f);

	    if (!Mouse.current.leftButton.isPressed) return;
	    if(Mouse.current.leftButton.wasPressedThisFrame) lastMousePos = localPos;
	    scratchBrushMaterial.SetVector("_PrevBrushUV", lastMousePos);
	    scratchBrushMaterial.SetVector("_BrushUV", localPos);
	    Graphics.Blit(Texture2D.whiteTexture, rt, scratchBrushMaterial);
	    lastMousePos = localPos;
    }
}
