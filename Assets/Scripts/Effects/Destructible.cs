using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

using Piece = System.Collections.Generic.List<UnityEngine.Vector2>;

namespace Effects {
	public class Destructible : MonoBehaviour {
		#region Fields

		[Header("Settings")]
		[SerializeField] private bool debug;
		[SerializeField] private PhysicsMaterial2D shardMat;
		[SerializeField] private float             explosionForce = 1.0f, fadeTime = 2.0f, fadeDelay = 2.0f;
		[SerializeField] private int               seedCount      = 3;

		private SpriteRenderer spriteRenderer;
		private Vector2[]      seeds;

		private          List<List<Piece>> rubberBandPoints, physicsShapePoints;
		private readonly Piece       physicsShapeList = new List<Vector2>();
		private          List<Vector2>       rubberBandList;


		private List<SpriteRenderer> shardRenderers = new List<SpriteRenderer>();

		private bool isRunning = false;

		#endregion

		#region Unity Functions

		private void Start() {
			isRunning       = true;
			spriteRenderer = GetComponent<SpriteRenderer>();
			seeds           = new Vector2[seedCount];

    		// More accurate bounds for irregular shapes.
			spriteRenderer.sprite.GetPhysicsShape(0, physicsShapeList);
			
			// Remove concave points from the shape since the triangulation algorithm only works with convex shapes.
			rubberBandList = RubberBand(physicsShapeList, GetLeftmostPoint(physicsShapeList));
			
			// Generate random seed points within the bounds of the sprite.
			for (int i = 0; i < seedCount; i++) {
				var tries = 0;
				while ((seeds[i] == new Vector2() || !IsPointInBand(rubberBandList, seeds[i])) && tries < 100) {
					tries++;
					seeds[i] = new Vector2(Random.Range(spriteRenderer.sprite.bounds.min.x,
					                                    spriteRenderer.sprite.bounds.max.x),
					                       Random.Range(spriteRenderer.sprite.bounds.min.y,
					                                    spriteRenderer.sprite.bounds.max.y));
				}
			}

			rubberBandPoints = GenShards(rubberBandList);
			physicsShapePoints = GenShards(physicsShapeList);
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
		
		/*
		private void OnDrawGizmos() {
			if (!isRunning || !debug) return;
			for (int i = 0; i < rubberBandPoints.Count; i++) {
				var points = this.rubberBandPoints[i];
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
			foreach (var point in physicsShapeList) {
				Gizmos.DrawSphere(transform.TransformPoint(point), 0.005f);
			}

			Gizmos.color = Color.yellow;
			foreach (var point in rubberBandList) {
				Gizmos.DrawSphere(transform.TransformPoint(point), 0.01f);
			}
		}
		*/
		
		#endregion

		#region Private Functions
	
		/// <summary>
		/// Cuts the polygon which is defined by the list of vertices, with the line defined by points A and B.
		/// </summary>
		/// <param name="points"> List of vertices</param>
		/// <param name="A"> First point, the sprite part that's on this side gets returned.</param>
		/// <param name="B"> Second point, the sprite part that's on this side gets discarded. </param>
		/// <returns>Returns a new polygon which is the remaining part of the original polygon after the cut.</returns>
		private List<Piece> Cut(Piece points, Vector2 A, Vector2 B) {
			// Indexes for entry and exit, used to make multiple polygons if there's a gap in the mesh.
			List<int> eIndex = new List<int>(), xIndex = new List<int>();
			
			var mid    = (A + B) / 2;
			var	along = new Vector2(-(B - A).y, (B - A).x);
			var cutPointOutline = new Piece();
			for (int i = 0; i < points.Count; i++) {
				if (Vector2.Dot(points[i] - mid, B - A) <= 0) {
					cutPointOutline.Add(points[i]);
					if (Vector2.Dot(points[(i + 1) % points.Count] - mid, B - A) > 0) {
						xIndex.Add(cutPointOutline.Count);
						cutPointOutline.Add(GetLine(A, B, points[i], points[(i + 1) % points.Count]));
					}
				} else {
					if (Vector2.Dot(points[(i + 1) % points.Count] - mid, B - A) <= 0) {
						eIndex.Add(cutPointOutline.Count);
						cutPointOutline.Add(GetLine(A, B, points[i], points[(i + 1) % points.Count]));
					}
				}
			}

			eIndex = eIndex.OrderBy(e => Vector2.Dot(cutPointOutline[e], along)).ToList();
			xIndex = xIndex.OrderBy(e => Vector2.Dot(cutPointOutline[e], along)).ToList();
			
			var mango = new int[cutPointOutline.Count];

			for (int i = 0; i < cutPointOutline.Count; i++) {
				mango[i] = (i + 1) % cutPointOutline.Count;
			}

			for (int i = 0; i < eIndex.Count; i++) {
				mango[xIndex[i]] = eIndex[i];
			}

			var visited = new bool[mango.Length];
			var result  = new List<Piece>();

			for (int i = 0; i < visited.Length; i++) {
				if (visited[i]) continue;
				var temp = new Piece { cutPointOutline[i] };
				visited[i] = true;
				var  current        = i;
				var completedPiece = false;
				while (!completedPiece) {
					current = mango[current];
					if (visited[current]) {
						completedPiece = true;
						continue;
					}
					temp.Add(cutPointOutline[current]);
					visited[current] = true;
				}
				result.Add(temp);
			}
			
			return result;
		}

		/// <summary>
		/// Checks if a point is inside a polygon defined by the list of vertices.
		/// </summary>
		/// <param name="band"> List of vertices that defines the polygon.</param>
		/// <param name="point"> Point that you want to check if it is inside or outside the polygon.</param>
		/// <returns> Return true if inside polygon, else false.</returns>
		private bool IsPointInBand(Piece band, Vector2 point) {
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
		private int GetLeftmostPoint(Piece points) {
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
		private Piece RubberBand(Piece points, int leftmost) {
			var result  = new Piece();
			var current = leftmost;


			for (int h = 0; h < points.Count(); h++) {
				var p     = points[current];
				var index = (current + points.Count / 2) % points.Count;
				var ig    = points[index];
				var pd    = (ig - p);
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
				if (child == spriteRenderer) continue;
				Destroy(child.gameObject);
			}
			
			int a = 0;
			foreach (var pieces in rubberBandPoints) {
				if (pieces.Count == 0 || physicsShapePoints[a].Count == 0) {
					a++;
					continue;
				}
				
				var shard = pieces[0];
				
				// Skip shards that don't have enough points to form a triangle.
				if (shard.Count < 3) {
					a++;
					continue;
				}
				
				// transform all points to make them work when making sprites.
				var vertices = new Piece(shard);
				for (int i = 0; i < vertices.Count; i++) {
					vertices[i] = vertices[i] * spriteRenderer.sprite.pixelsPerUnit + spriteRenderer.sprite.pivot;
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
				var sprite = Sprite.Create(spriteRenderer.sprite.texture, spriteRenderer.sprite.rect,
				                           spriteRenderer.sprite.pivot / spriteRenderer.sprite.rect.size,
				                           spriteRenderer.sprite.pixelsPerUnit, 0, SpriteMeshType.FullRect
				                           );
				sprite.OverrideGeometry(vertices.ToArray(), triangles.ToArray());
				renderer.sprite = sprite;
				if (debug) renderer.color = Color.HSVToRGB(Mathf.Repeat(seeds[a].x * 43176.1746f, 1f), 1f, 1f);
				collider.pathCount = physicsShapePoints[a].Count;
				for (int i = 0; i < physicsShapePoints[a].Count; i++) {
					collider.SetPath(i, physicsShapePoints[a][i]);
				}
				
				obj.transform.SetParent(transform, false);
				collider.sharedMaterial = shardMat;
				
				// Make the shards explode from the center.
				var dir = (new Vector3(pos.x, pos.y, 0) - transform.position).normalized;
				rigidbody.AddForce((dir + forceDir).normalized * explosionForce, ForceMode2D.Impulse);
				a++;
				
				// Make the shards disappear after a few seconds.
				shardRenderers.Add(renderer);
			}
			
			LeanTween.value(gameObject, 1, 0, fadeTime).setDelay(fadeDelay).setDestroyOnComplete(true).setOnUpdate(e => {foreach
				(var renderer in shardRenderers) {
				var col = renderer.color;
				col.a = e;
				renderer.color = col;
			}});
			
			// Disable the collider and spriteRenderer so they don't collide
			gameObject.GetComponent<Collider2D>().enabled = false;
			spriteRenderer.enabled                          = false;
		}

		
		// Generate a list of shards
		private List<List<Piece>> GenShards(Piece points) {
			var           result = new List<List<Piece>>();
			var           list   = new Piece(points);
			for (int i = 0; i < seeds.Length; i++) {
				var copy = new List<Piece> { list };
				for (int j = 0; j < seeds.Length; j++) {
					if (j == i) continue;
					var temp = new List<Piece>();
					foreach (var piece in copy) {
						temp.AddRange(Cut(piece, seeds[i], seeds[j]));
					}
					copy = temp;
				}

				result.Add(copy);
			}

			return result;
		}

		#endregion
	}
}