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
        thunderMat.SetFloat("_Width", transform.localScale.x);
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
	    
	    thunderMat.SetFloat("_Reveal", 0.0f);
	    thunderMat.SetFloat("_Flash", 0.3f);
	    
	    LeanTween.value(gameObject, 0.0f, 1.0f, 0.12f).setOnUpdate(e => thunderMat.SetFloat("_Reveal", e));
	    
	    yield return new WaitForSeconds(0.12f);
	    
	    float dist = Random.Range(300.0f, 3000.0f);
	    StartCoroutine(PlayThunderSound(dist));
	    
	    int strikes = Random.Range(0, 3);
	    for (int i = 0; i < strikes; i++) {
		    Strike(0.1f);
		    yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
	    }
	    Strike(0.4f);
    }

    private IEnumerator PlayThunderSound(float dist) {
	    yield return new WaitForSeconds(dist / 343.0f);

	    var instance = RuntimeManager.CreateInstance(thunderSound);
	    instance.setParameterByName("StrikeDistance", dist);
	    instance.start();
	    instance.release();
    }

    
}
