using UnityEngine;

[ExecuteAlways]
public class CloudLightingRenderer : MonoBehaviour {
	[SerializeField] private SpriteRenderer moonSr;
	[SerializeField] private Material       CloudLightMat;
	[SerializeField] private Texture2D      SdfTexture;

	private RenderTexture renderTexture;
	
	private void OnEnable() {
		renderTexture = new RenderTexture(SdfTexture.width, SdfTexture.height, 0, RenderTextureFormat.RFloat);
		renderTexture.filterMode = FilterMode.Point;
		renderTexture.wrapMode = TextureWrapMode.Clamp;
		renderTexture.Create();
	}
	
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
