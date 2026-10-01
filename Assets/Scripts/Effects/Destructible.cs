using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Effects {
	public class Destructible : MonoBehaviour {
		[SerializeField] private        float     explosionForce = 1;
		[SerializeField] private        Vector2   p0             = new Vector2(0, 0);
		[SerializeField] private        Vector2   p1             = new Vector2(1, 1);
		[SerializeField] private int       seedCount      = 3;
		private                         Vector2[] seeds;

		[SerializeField] private PhysicsMaterial2D shardMat;

		private SpriteRenderer _spriteRenderer;
		private Sprite[]       _shards = new Sprite[4];

		private bool isRunning = false;
		
		private void OnMouseDown() => Shatter();

		private List<Vector2> Cut(List<Vector2> points, Vector2 A, Vector2 B) {
			var mid    = (A + B) / 2;
			var result = new List<Vector2>();
			for (int i = 0; i < points.Count; i++) {
				if (Vector2.Dot(points[i] - mid, B - A) <= 0) {
					result.Add(points[i]);
					if (Vector2.Dot(points[(i + 1) % points.Count] - mid, B - A) > 0) {
						result.Add(GetLine(A, B, points[i], points[(i + 1) % points.Count]));
					}
				} else {
					if (Vector2.Dot(points[(i + 1) % points.Count] - mid, B - A) <= 0) {
						result.Add(GetLine(A, B, points[i], points[(i + 1) % points.Count]));
					}
				}
			}

			return result;
		}

		private Vector2 GetLine(Vector2 A, Vector2 B, Vector2 P0, Vector2 P1) {
			var arrow    = B - A;
			var mid      = new Vector2(A.x + B.x, A.y + B.y) / 2;
			var gap      = Vector2.Dot(mid - P0, arrow);
			var step     = Vector2.Dot(P1  - P0, arrow);
			var t        = gap / step;
			var crossing = P0 + t * (P1 - P0);

			return crossing;
		}

		private void Shatter() {
			foreach (var child in gameObject.GetComponentsInChildren<SpriteRenderer>()) {
				if (child == _spriteRenderer) continue;
				Destroy(child.gameObject);
			}

			print("Youch I got shattered");
			print($"rect: {_spriteRenderer.sprite.rect}, pivot: {_spriteRenderer.sprite.pivot}, PixelsPerUnit: {_spriteRenderer.sprite.pixelsPerUnit}");

			var list = GenShards();
			

			foreach (var shard in list) {
				var vertices = new List<Vector2>(shard);
				for (int i = 0; i < vertices.Count; i++) {
					vertices[i] = vertices[i] * _spriteRenderer.sprite.pixelsPerUnit + _spriteRenderer.sprite.pivot;
				}
				
				List<ushort> triangles = new List<ushort>();
				for (int i = 1; i <= vertices.Count - 2; i++) {
					triangles.Add(0);
					triangles.Add((ushort)i);
					triangles.Add((ushort)(i + 1));
				}
				
				var obj      = new GameObject("shard");
				var collider = obj.AddComponent<PolygonCollider2D>();
				var rigidbody = obj.AddComponent<Rigidbody2D>();
				var renderer = obj.AddComponent<SpriteRenderer>();
				var sprite = Sprite.Create(_spriteRenderer.sprite.texture, _spriteRenderer.sprite.rect, _spriteRenderer.sprite.pivot / _spriteRenderer.sprite.rect.size, _spriteRenderer.sprite.pixelsPerUnit);
				sprite.OverrideGeometry(vertices.ToArray(), triangles.ToArray());
				renderer.sprite =  sprite;
				collider.points = shard.ToArray();
				obj.transform.SetParent(transform, false);
			}
			
			/*_shards[0] = Sprite.Create(_spriteRenderer.sprite.texture,
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
			*/
			gameObject.GetComponent<BoxCollider2D>().enabled = false;
			_spriteRenderer.enabled                          = false;
		}

		private void Start() {
			isRunning       = true;
			_spriteRenderer = GetComponent<SpriteRenderer>();
			seeds           = new Vector2[seedCount];
			for (int i = 0; i < seedCount; i++) {
				seeds[i] = new Vector2(Random.Range(_spriteRenderer.sprite.bounds.min.x,
				                                      _spriteRenderer.sprite.bounds.max.x),
				                         Random.Range(_spriteRenderer.sprite.bounds.min.y,
				                                      _spriteRenderer.sprite.bounds.max.y));
			}
		}

		private List<List<Vector2>> GenShards() {
			var result = new List<List<Vector2>>();
			List<Vector2> list = new List<Vector2>();
			list.Add(new Vector2(_spriteRenderer.sprite.bounds.min.x, _spriteRenderer.sprite.bounds.min.y));
			list.Add(new Vector2(_spriteRenderer.sprite.bounds.max.x, _spriteRenderer.sprite.bounds.min.y));
			list.Add(new Vector2(_spriteRenderer.sprite.bounds.max.x, _spriteRenderer.sprite.bounds.max.y));
			list.Add(new Vector2(_spriteRenderer.sprite.bounds.min.x, _spriteRenderer.sprite.bounds.max.y));
			for (int i = 0; i < seeds.Length; i++) {
				var copy = new List<Vector2>(list);
				for (int j = 0; j < seeds.Length; j++) {
					if (j == i) continue;
					copy = Cut(copy, seeds[i], seeds[j]);
				}
				result.Add(copy);
			}

			return result;
		}

		private void OnDrawGizmos() {
			if (!isRunning) return;
			var list = GenShards();
			for (int i = 0; i < list.Count; i++) {
				Gizmos.color = Color.HSVToRGB((float)i / seeds.Length, 1f, 1f);
				Gizmos.DrawSphere(seeds[i], 0.005f);
				for (int j = 0; j < list[i].Count; j++) {
					Gizmos.DrawLine(transform.TransformPoint(list[i][j]),
					                transform.TransformPoint(list[i][(j + 1) % list[i].Count]));
				}
			}
		}
	}
}