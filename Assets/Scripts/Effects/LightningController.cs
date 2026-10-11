using System.Collections;
using UnityEngine;
using FMODUnity;

public class LightningController : MonoBehaviour {

	
	[SerializeField] private SpriteRenderer flashRenderer;
	[SerializeField] private EventReference thunderSound;
	
	private Material thunderMat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        thunderMat = GetComponent<SpriteRenderer>().material;
        
    }

    [ContextMenu("Strike")]
    public void Strike(float duration) {
	    LeanTween.value(gameObject, 1.0f, 0.0f, duration).setOnUpdate(e => {
		                                                                  thunderMat.SetFloat("_Flash", e);
								                                              var col =  flashRenderer.color;
		                                                                  col.a = e * 0.4f;
		                                                                  flashRenderer.color = col;
	                                                                  }).setEase(LeanTweenType.easeOutExpo).setOnComplete(e => thunderMat.SetFloat("_Flash", 0.0f));
    }

    public IEnumerator StrikeSequence(float x) {
	    transform.localPosition = new Vector3(x, 0, 10.0f);
	    thunderMat.SetFloat("_Seed", Random.Range(0, 100.0f));
	    int strikes = Random.Range(0, 3);
	    for (int i = 0; i < strikes; i++) {
		    Strike(0.1f);
		    yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
	    }
	    Strike(0.4f);
    }

    
}
