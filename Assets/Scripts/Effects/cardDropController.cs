using Logic;
using UI;
using UnityEngine;

public class cardDropController : MonoBehaviour
{
	private void Start() {
		LeanTween.scaleX(gameObject, 0f, 3f).setLoopPingPong();
		LeanTween.moveLocalY(gameObject, transform.localPosition.y + 0.5f, 2f).setLoopPingPong().setEaseInOutSine();
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		if (!collision.gameObject.CompareTag("Player")) return;
		GameManager.Instance.cardCount++;
		GameUIManager.Instance.UpdateScratchCardUI();
		Destroy(gameObject);
	}
}
