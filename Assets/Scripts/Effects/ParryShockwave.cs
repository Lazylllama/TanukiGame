using UnityEngine;

namespace Effects {
	public class ParryShockwave : MonoBehaviour {
		private static readonly  int   Progress = Shader.PropertyToID("_Progress");
		[SerializeField] private float duration = 0.35f;

		private SpriteRenderer        spriteRenderer;
		private MaterialPropertyBlock block;

		private void Awake() {
			spriteRenderer = GetComponent<SpriteRenderer>();
			block          = new MaterialPropertyBlock();
			SetProgress(0f);

			LeanTween.value(gameObject, 0f, 1f, duration).setOnUpdate(SetProgress)
			         .setEaseOutQuad()
			         .setOnUpdate(SetProgress)
			         .setOnComplete(() => Destroy(gameObject));
		}

		private void SetProgress(float value) {
			spriteRenderer.GetPropertyBlock(block);
			block.SetFloat(Progress, value);
			spriteRenderer.SetPropertyBlock(block);
		}
	}
}