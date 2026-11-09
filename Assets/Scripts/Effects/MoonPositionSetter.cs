using UnityEngine;

[ExecuteAlways]
public class MoonPositionSetter : MonoBehaviour {

	[SerializeField] private GameObject     moon;
	[SerializeField] private SpriteRenderer sr;
	
	[ExecuteAlways]
    void Update() {
	    Shader.SetGlobalVector("_MoonPos", moon.GetComponent<SpriteRenderer>().bounds.center);
    }
}
