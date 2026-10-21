using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

using Piece = System.Collections.Generic.List<UnityEngine.Vector2>;

public class DeathShatterManager : MonoBehaviour {

	[SerializeField] private int seedCount = 10;
	[SerializeField] private SpriteRenderer background;
	
	private SpriteRenderer spriteRenderer;
	private List<Vector2>  spritePoints = new List<Vector2>();
	private Texture2D      screenShot;
	private Vector2[]      seeds;
	
	[ContextMenu("Grab Screenshot")]
	private void GrabScreenshot() {
		spriteRenderer = GetComponent<SpriteRenderer>();
		
		StartCoroutine(GrabScreenShot());
	}

	IEnumerator GrabScreenShot() {
		yield return new WaitForEndOfFrame();
		
		screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
		screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		screenShot.Apply();
		
		var pixelsPerUnit = (float)screenShot.height / Camera.main.orthographicSize * 2;
		
		spriteRenderer.sprite = Sprite.Create(screenShot, new Rect(0, 0, screenShot.width, screenShot.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);

		yield return null;
		
		Shatter();
	}

	private void Shatter() {
		seeds = new Vector2[seedCount];
		
		for (int i = 0; i < seedCount; i++) {
			seeds[i] = new Vector2(Random.Range(spriteRenderer.sprite.bounds.min.x,
			                                    spriteRenderer.sprite.bounds.max.x),
			                       Random.Range(spriteRenderer.sprite.bounds.min.y,
			                                    spriteRenderer.sprite.bounds.max.y));
		}

		spritePoints.Clear();
		spritePoints.Add(spriteRenderer.sprite.bounds.max);
		spritePoints.Add(new Vector2(spriteRenderer.sprite.bounds.max.x, spriteRenderer.sprite.bounds.min.y));
		spritePoints.Add(spriteRenderer.sprite.bounds.min);
		spritePoints.Add(new Vector2(spriteRenderer.sprite.bounds.min.x, spriteRenderer.sprite.bounds.max.y));
		
		var listOfListOfShards = GenShards(spritePoints);

		int a = 0;
		foreach (var listOfShards in listOfListOfShards) {
			if (listOfShards.Count == 0) {
				a++;
				continue;
			}
			
			var shard = listOfShards[0];

			if (shard.Count < 3) {
				a++;
				continue;
			}

			var vertices = new Piece(shard);
			for (int i = 0; i < vertices.Count; i++) {
				vertices[i] = vertices[i] * spriteRenderer.sprite.pixelsPerUnit + spriteRenderer.sprite.pivot;
			}
			
			List<ushort> triangles = new List<ushort>();
			for(int i = 1; i <= vertices.Count - 2; i++) {
				triangles.Add(0);
				triangles.Add((ushort)i);
				triangles.Add((ushort)(i + 1));
			}
			
			var pos = Vector2.zero;
			foreach (var vert in shard) {
				pos.x += vert.x;
				pos.y += vert.y;
			}
			
			pos /= shard.Count;
			
			pos = transform.TransformPoint(pos);
			
			var obj       = new GameObject("shard");
			var rigidbody = obj.AddComponent<Rigidbody2D>();
			var renderer  = obj.AddComponent<SpriteRenderer>();
			var sprite = Sprite.Create(spriteRenderer.sprite.texture, spriteRenderer.sprite.rect,
			                           spriteRenderer.sprite.pivot / spriteRenderer.sprite.rect.size,
			                           spriteRenderer.sprite.pixelsPerUnit, 0, SpriteMeshType.FullRect
			                          );
			sprite.OverrideGeometry(vertices.ToArray(), triangles.ToArray());
			renderer.sprite = sprite;
			obj.transform.SetParent(transform, false);



			renderer.sortingLayerID = SortingLayer.NameToID("DeathShatter");
			
			var dir = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1));
			rigidbody.AddForce(dir, ForceMode2D.Impulse);
		}
		spriteRenderer.enabled = false;

		GameUIManager.Instance.gameObject.SetActive(false);
		InitBackGround();
	}

	private void InitBackGround() {
		background.enabled = true;
		background.transform.localScale = new Vector3((Camera.main.orthographicSize) * Camera.main.aspect,
		                                              Camera.main.orthographicSize, 1f);
	}
	
	private List<List<Piece>> GenShards(Piece points) {
		var result = new List<List<Piece>>();
		var list   = new Piece(points);
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
	
	private List<Piece> Cut(Piece points, Vector2 A, Vector2 B) {
		// Indexes for entry and exit, used to make multiple polygons if there's a gap in the mesh.
		List<int> eIndex = new List<int>(), xIndex = new List<int>();

		var mid             = (A + B) / 2;
		var along           = new Vector2(-(B - A).y, (B - A).x);
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
			var current        = i;
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
	
	private Vector2 GetLine(Vector2 A, Vector2 B, Vector2 P0, Vector2 P1) {
		var arrow    = B - A;
		var mid      = new Vector2(A.x + B.x, A.y + B.y) / 2;
		var gap      = Vector2.Dot(mid - P0, arrow);
		var step     = Vector2.Dot(P1  - P0, arrow);
		var t        = gap / step;
		var crossing = P0 + t * (P1 - P0);

		return crossing;
	}

	private void OnDrawGizmos() {
		if(seeds == null) return;
		foreach (var seed in seeds) {
			Gizmos.DrawSphere(transform.TransformPoint(seed), 0.1f);
		}
		Gizmos.color = Color.red;
		foreach (var point in spritePoints) {
			Gizmos.DrawSphere(transform.TransformPoint(point), 0.1f);
		}
	}
}
