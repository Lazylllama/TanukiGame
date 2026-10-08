using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Effects {
	public class Destructible : MonoBehaviour {
		#region Fields

		[Header("Settings")]
		[SerializeField] private bool debug;
		[SerializeField] private PhysicsMaterial2D shardMat;
		[SerializeField] private float             explosionForce = 1;
		[SerializeField] private int               seedCount      = 3;

		private SpriteRenderer _spriteRenderer;
		private Vector2[]      seeds;

		private List<List<Vector2>> _points;
		private List<Vector2>       _physicsShapePoints = new List<Vector2>(), rubberBandList;
		private List<GameObject> 	 shards = new List<GameObject>();

		private bool isRunning = false;

		#endregion

		#region Unity Functions

		private void Start() {
			isRunning       = true;
			_spriteRenderer = GetComponent<SpriteRenderer>();
			seeds           = new Vector2[seedCount];

    		// More accurate bounds for irregular shapes.
			_spriteRenderer.sprite.GetPhysicsShape(0, _physicsShapePoints);
			
			// Remove concave points from the shape since the triangulation algorithm only works with convex shapes.
			rubberBandList = RubberBand(_physicsShapePoints, GetLeftmostPoint(_physicsShapePoints));
			
			// Generate random seed points within the bounds of the sprite.
			for (int i = 0; i < seedCount; i++) {
				var tries = 0;
				while ((seeds[i] == new Vector2() || !IsPointInBand(rubberBandList, seeds[i])) && tries < 100) {
					tries++;
					seeds[i] = new Vector2(Random.Range(_spriteRenderer.sprite.bounds.min.x,
					                                    _spriteRenderer.sprite.bounds.max.x),
					                       Random.Range(_spriteRenderer.sprite.bounds.min.y,
					                                    _spriteRenderer.sprite.bounds.max.y));
				}
			}

			_points = GenShards();
		}
		

		private void OnMouseDown() {
			// Get the mouse position in screen space,
			var mouseScreenPos = Mouse.current.position.ReadValue();

			if (Camera.main == null) return;
			
			// Convert the mouse position to world space and set z to 0.
			var mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
			mouseWorldPos.z = 0;
			
			// Get the direction from the click point to the center of the object and use as the explosion direction.
			var dir = (transform.position - mouseWorldPos).normalized * 10.0f;
			Shatter(dir);
		}

		private void OnDrawGizmos() {
			if (!isRunning || !debug) return;
			for (int i = 0; i < _points.Count; i++) {
				var points = _points[i];
				Gizmos.color = Color.HSVToRGB(Mathf.Repeat(seeds[i].x * 43176.1746f, 1f), 1f, 1f);
				for (int j = 0; j < points.Count; j++) {
					Gizmos.DrawLine(transform.TransformPoint(points[j]),
					                transform.TransformPoint(points[(j + 1) % points.Count]));
				}
			}

			for (int i = 0; i < seeds.Length; i++) {
				var seed = seeds[i];
				Gizmos.color = Color.HSVToRGB(Mathf.Repeat(seeds[i].x * 43176.1746f, 1f), 1f, 1f);
				Gizmos.DrawSphere(transform.TransformPoint(seed), 0.01f);
			}

			Gizmos.color = Color.purple;
			foreach (var point in _physicsShapePoints) {
				Gizmos.DrawSphere(transform.TransformPoint(point), 0.005f);
			}

			Gizmos.color = Color.yellow;
			foreach (var point in rubberBandList) {
				Gizmos.DrawSphere(transform.TransformPoint(point), 0.01f);
			}
		}

		#endregion

		#region Private Functions
	
		/// <summary>
		/// Cuts the polygon which is defined by the list of vertices, with the line defined by points A and B.
		/// </summary>
		/// <param name="points"> List of vertices</param>
		/// <param name="A"> First point, the sprite part that's on this side gets returned.</param>
		/// <param name="B"> Second point, the sprite part that's on this side gets discarded. </param>
		/// <returns>Returns a new polygon which is the remaining part of the original polygon after the cut.</returns>
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

		/// <summary>
		/// Checks if a point is inside a polygon defined by the list of vertices.
		/// </summary>
		/// <param name="band"> List of vertices that defines the polygon.</param>
		/// <param name="point"> Point that you want to check if it is inside or outside the polygon.</param>
		/// <returns> Return true if inside polygon, else false.</returns>
		private bool IsPointInBand(List<Vector2> band, Vector2 point) {
			for (int i = 0; i < band.Count; i++) {
				var d = band[(i + 1) % band.Count()] - band[i];
				d = new Vector2(-d.y, d.x);
				if (Vector2.Dot(point - band[i], d) < 0) return false;
			}

			return true;
		}

		/// <summary>
		/// Gets the index of the leftmost point in a list of points. This is used to find a starting point for the rubber band algorithm.
		/// </summary>
		/// <param name="points"> List of points that you want to find the leftmost point in.</param>
		/// <returns> Return the index of the leftmost point.</returns>
		private int GetLeftmostPoint(List<Vector2> points) {
			var index       = 0;
			var currentBest = points[0];
			for (int i = 0; i < points.Count; i++) {
				if (points[i].x < currentBest.x) {
					currentBest = points[i];
					index       = i;
				}
			}

			return index;
		}

		/// <summary>
		/// Rubber band algorithm, used to find the convex hull of a set of points.
		/// Used since the physics shape of a sprite can be concave, which is not supported by the triangulation algorithm.
		/// </summary>
		/// <param name="points"> List of vertices that define the current polygon. </param>
		/// <param name="leftmost"> The index of the leftmost vertice, found using GetLeftMostPoint(). </param>
		/// <returns> Returns a new polygon made of the convex hull of the inputted polygon. </returns>
		private List<Vector2> RubberBand(List<Vector2> points, int leftmost) {
			var result  = new List<Vector2>();
			var current = leftmost;
			var index   = (current + points.Count / 2) % points.Count;


			for (int h = 0; h < points.Count(); h++) {
				var p = points[current];
				index = (current + points.Count / 2) % points.Count;
				var ig = points[index];
				var pd = (ig - p);
				pd = new Vector2(-pd.y, pd.x);
				for (int i = 0; i < points.Count; i++) {
					var g  = points[i];
					var gd = (g - p);
					if (Vector2.Dot(pd, gd) < 0) {
						pd    = new Vector2(-gd.y, gd.x);
						index = i;
					}
				}

				result.Add(points[index]);
				current = index;
				if (current == leftmost) break;
			}

			return result;
		}

		/// <summary>
		/// Finds the intersection point of a line that goes through points P0 and P1. With the perpendicular bisector of A and B.
		/// </summary>
		/// <param name="A">First point that defines the bisector.</param>
		/// <param name="B">Second point that defines the bisector.</param>
		/// <param name="P0">Start point of the line being intersected.</param>
		/// <param name="P1">End point of the line being intersected.</param>
		/// <returns> Returns </returns>
		private Vector2 GetLine(Vector2 A, Vector2 B, Vector2 P0, Vector2 P1) {
			var arrow    = B - A;
			var mid      = new Vector2(A.x + B.x, A.y + B.y) / 2;
			var gap      = Vector2.Dot(mid - P0, arrow);
			var step     = Vector2.Dot(P1  - P0, arrow);
			var t        = gap / step;
			var crossing = P0 + t * (P1 - P0);

			return crossing;
		}

		private void Shatter(Vector3 forceDir = new Vector3()) {
			// Clean up old shards in case the function is called multiple times.
			foreach (var child in gameObject.GetComponentsInChildren<SpriteRenderer>()) {
				if (child == _spriteRenderer) continue;
				Destroy(child.gameObject);
			}
			
			int a = 0;
			foreach (var shard in _points) {
				// Skip shards that don't have enough points to form a triangle.
				if (shard.Count < 3) {
					a++;
					continue;
				}
				
				// transform all points to make them work when making sprites.
				var vertices = new List<Vector2>(shard);
				for (int i = 0; i < vertices.Count; i++) {
					vertices[i] = vertices[i] * _spriteRenderer.sprite.pixelsPerUnit + _spriteRenderer.sprite.pivot;
				}
				
				// Triangle fanning to make a sprite out of triangles.
				List<ushort> triangles = new List<ushort>();
				for (int i = 1; i <= vertices.Count - 2; i++) {
					triangles.Add(0);
					triangles.Add((ushort)i);
					triangles.Add((ushort)(i + 1));
				}

				// Get the position of the shard by averaging its points.
				var pos = Vector2.zero;
				foreach (var vert in shard) {
					pos.x += vert.x;
					pos.y += vert.y;
				}

				pos.x /= vertices.Count;
				pos.y /= vertices.Count;

				pos = transform.TransformPoint(pos);

				// Create the gameObject and give it a collider, spriteRenderer and rigidbody.
				var obj       = new GameObject("shard");
				var collider  = obj.AddComponent<PolygonCollider2D>();
				var rigidbody = obj.AddComponent<Rigidbody2D>();
				var renderer  = obj.AddComponent<SpriteRenderer>();
				var sprite = Sprite.Create(_spriteRenderer.sprite.texture, _spriteRenderer.sprite.rect,
				                           _spriteRenderer.sprite.pivot / _spriteRenderer.sprite.rect.size,
				                           _spriteRenderer.sprite.pixelsPerUnit);
				sprite.OverrideGeometry(vertices.ToArray(), triangles.ToArray());
				renderer.sprite = sprite;
				if (debug) renderer.color = Color.HSVToRGB(Mathf.Repeat(seeds[a].x * 43176.1746f, 1f), 1f, 1f);
				collider.points = shard.ToArray();
				obj.transform.SetParent(transform, false);
				collider.sharedMaterial = shardMat;
				
				// Make the shards explode from the center.
				var dir = (new Vector3(pos.x, pos.y, 0) - transform.position).normalized;
				rigidbody.AddForce((dir + forceDir).normalized * explosionForce, ForceMode2D.Impulse);
				shards.Add(obj);
				a++;
			}
			
			// Disable the collider and spriteRenderer so they don't collide
			gameObject.GetComponent<Collider2D>().enabled = false;
			_spriteRenderer.enabled                          = false;
		}

		
		// Generate a list of shards
		private List<List<Vector2>> GenShards() {
			var           result = new List<List<Vector2>>();
			List<Vector2> list   = new List<Vector2>(rubberBandList);
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

		#endregion
	}
}