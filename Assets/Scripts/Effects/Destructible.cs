using UnityEngine;

namespace Effects {
	public class Destructible : MonoBehaviour {
		[SerializeField] private float explosionForce = 1;

		[SerializeField] private PhysicsMaterial2D shardMat; 
	
		private SpriteRenderer _spriteRenderer;
		private Sprite[]       _shards = new Sprite[4];

		private void OnMouseDown() => Shatter();

		private void Shatter() {
			foreach (var child in gameObject.GetComponentsInChildren<SpriteRenderer>()) {
				if (child == _spriteRenderer) continue;
				Destroy(child.gameObject);
			}

			print("Youch I got shattered");
			print($"rect: {_spriteRenderer.sprite.rect}, pivot: {_spriteRenderer.sprite.pivot}, PixelsPerUnit: {_spriteRenderer.sprite.pixelsPerUnit}");
			_shards[0] = Sprite.Create(_spriteRenderer.sprite.texture,
			                           new Rect(_spriteRenderer.sprite.rect.x, _spriteRenderer.sprite.rect.y,
			                                    _spriteRenderer.sprite.rect.width  / 2,
			                                    _spriteRenderer.sprite.rect.height / 2),
			                           new Vector2(0.5f, 0.5f), _spriteRenderer.sprite.pixelsPerUnit);
			_shards[1] = Sprite.Create(_spriteRenderer.sprite.texture,
			                           new Rect(_spriteRenderer.sprite.rect.x + _spriteRenderer.sprite.rect.width / 2,
			                                    _spriteRenderer.sprite.rect.y, _spriteRenderer.sprite.rect.width / 2,
			                                    _spriteRenderer.sprite.rect.height                               / 2),
			                           new Vector2(0.5f, 0.5f), _spriteRenderer.sprite.pixelsPerUnit);
			_shards[2] = Sprite.Create(_spriteRenderer.sprite.texture,
			                           new Rect(_spriteRenderer.sprite.rect.x,
			                                    _spriteRenderer.sprite.rect.y + _spriteRenderer.sprite.rect.height / 2,
			                                    _spriteRenderer.sprite.rect.width  / 2,
			                                    _spriteRenderer.sprite.rect.height / 2),
			                           new Vector2(0.5f, 0.5f), _spriteRenderer.sprite.pixelsPerUnit);
			_shards[3] = Sprite.Create(_spriteRenderer.sprite.texture,
			                           new Rect(_spriteRenderer.sprite.rect.x + _spriteRenderer.sprite.rect.width  / 2,
			                                    _spriteRenderer.sprite.rect.y + _spriteRenderer.sprite.rect.height / 2,
			                                    _spriteRenderer.sprite.rect.width  / 2,
			                                    _spriteRenderer.sprite.rect.height / 2),
			                           new Vector2(0.5f, 0.5f), _spriteRenderer.sprite.pixelsPerUnit);
			for (int i = 0; i < _shards.Length; i++) {
				var shard       = _shards[i];
				var shardObject = new GameObject("Shard");
				var renderer    = shardObject.AddComponent<SpriteRenderer>();
				renderer.sprite = shard;
				var cellCenterPx = shard.rect.center - _spriteRenderer.sprite.rect.position;
				var localPos     = (cellCenterPx - _spriteRenderer.sprite.pivot) / _spriteRenderer.sprite.pixelsPerUnit;
				shardObject.transform.position = transform.TransformPoint(localPos);
				shardObject.transform.SetParent(gameObject.transform);
				var bc = shardObject.AddComponent<BoxCollider2D>();
				var rb = shardObject.AddComponent<Rigidbody2D>();
				bc.sharedMaterial = shardMat;
				var dir = (shardObject.transform.position - transform.position).normalized;
				rb.AddForce(dir * explosionForce, ForceMode2D.Impulse);
			}

			gameObject.GetComponent<BoxCollider2D>().enabled = false;
			_spriteRenderer.enabled                          = false;
		}

		private void Start() {
			_spriteRenderer = GetComponent<SpriteRenderer>();
		}
	}
}