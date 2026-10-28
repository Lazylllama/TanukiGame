using TMPro;
using UnityEngine;

namespace Effects {
	public class DamageIndicator : MonoBehaviour {
		[SerializeField] private float riseHeight = 1f;
		[SerializeField] private float lifeTime   = 0.5f;

		public static void Spawn(DamageIndicator prefab, Vector3 position, float damage) {
			var offset = new Vector3(Random.Range(-0.3f, 0.3f), 0.5f, 0f);
			var number = Instantiate(prefab, position + offset, Quaternion.identity);
			number.Play(damage);
		}

		private void Play(float damage) {
			var text = GetComponent<TextMeshPro>();
			text.text = Mathf.RoundToInt(damage).ToString();

			Lib.Feel.Punch(transform, transform.localScale, 0.6f, 0.25f);
			LeanTween.moveY(gameObject, transform.position.y + riseHeight, lifeTime).setEaseOutQuad();
			LeanTween.value(gameObject, 1f, 0f, lifeTime * 0.4f)
			         .setDelay(lifeTime                  * 0.6f)
			         .setOnUpdate(alpha => text.alpha = alpha)
			         .setOnComplete(() => Destroy(gameObject));
		}
	}
}