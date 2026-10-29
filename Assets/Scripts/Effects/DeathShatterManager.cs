using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Player;
using UI;
using UnityEngine;
using Piece = System.Collections.Generic.List<UnityEngine.Vector2>;
using Random = UnityEngine.Random;

public class DeathShatterManager : MonoBehaviour {
	public static DeathShatterManager instance;

	[SerializeField, Min(1)] private int            seedCount = 10;
	[SerializeField] private SpriteRenderer background;
	[SerializeField] private float          shatterSpeed = 0.08f, shatterDelay = 0.2f, transitionDelay = 5f, shatterForce = 3f, seedSpread = 1.2f, shardTintLow = 0.7f;
	
	private SpriteRenderer spriteRenderer;
	private List<Vector2>  spritePoints = new List<Vector2>();
	private Texture2D      screenShot;
	private Vector3        tanukiPos;
	private Vector2[]      seeds;
	private Camera mainCamera;
	private List<Sprite> sprites = new List<Sprite>();
	

	private void Awake() {
		if (instance == null) {
			instance = this;
		} else {
			Destroy(gameObject);
		}
		mainCamera =  Camera.main;
	}

	private void OnDestroy() {
		Destroy(screenShot);
		foreach (var sprite in sprites) {
			Destroy(sprite);
		}
	}

	[ContextMenu("Grab Screenshot")]
	public void GrabScreenshot(Action callback = null) {
		spriteRenderer = GetComponent<SpriteRenderer>();
		StartCoroutine(TransitionTimer(callback));
		StartCoroutine(GrabScreenShot());
	}

	IEnumerator TransitionTimer(Action callback) {
		yield return new WaitForSeconds(transitionDelay);
		callback?.Invoke();
	}

	IEnumerator GrabScreenShot() {
		yield return new WaitForEndOfFrame();

		screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
		screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		screenShot.Apply();
		
		var pixelsPerUnit = (float)screenShot.height / mainCamera.orthographicSize * 2;
		
		spriteRenderer.sprite = Sprite.Create(screenShot, new Rect(0, 0, screenShot.width, screenShot.height), new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.FullRect);
		tanukiPos = transform.InverseTransformPoint(PlayerController.Instance.transform.position);
		yield return null;

		Shatter();
	}

	private void Shatter() {
		seeds = new Vector2[seedCount];

		for (int i = 0; i < seedCount; i++) {
			var angle  = Random.Range(0, 360);
			var offset = new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad));
			offset *= Mathf.Pow(Random.Range(0f, 1f), 2) * mainCamera.orthographicSize * mainCamera.aspect * seedSpread / transform.localScale.x;

			seeds[i] = new Vector2(offset.x + tanukiPos.x, offset.y + tanukiPos.y);
		}

		spritePoints.Clear();
		spritePoints.Add(spriteRenderer.sprite.bounds.max);
		spritePoints.Add(new Vector2(spriteRenderer.sprite.bounds.max.x, spriteRenderer.sprite.bounds.min.y));
		spritePoints.Add(spriteRenderer.sprite.bounds.min);
		spritePoints.Add(new Vector2(spriteRenderer.sprite.bounds.min.x, spriteRenderer.sprite.bounds.max.y));

		var listOfListOfShards = GenShards(spritePoints);
		
		foreach (var listOfShards in listOfListOfShards) {
			if (listOfShards.Count == 0) continue;
			
			var shard = listOfShards[0];

			if (shard.Count < 3) continue;
			

			var vertices = new Piece(shard);
			for (int i = 0; i < vertices.Count; i++) {
				vertices[i] = vertices[i] * spriteRenderer.sprite.pixelsPerUnit + spriteRenderer.sprite.pivot;
			}

			List<ushort> triangles = new List<ushort>();
			for (int i = 1; i <= vertices.Count - 2; i++) {
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


			var obj = new GameObject("shard");
			obj.transform.SetParent(transform, false);
			obj.transform.localPosition = pos;

			var rigidbody = obj.AddComponent<Rigidbody2D>();
			var renderer  = obj.AddComponent<SpriteRenderer>();
			var sprite = Sprite.Create(spriteRenderer.sprite.texture, spriteRenderer.sprite.rect,
			                           spriteRenderer.sprite.pivot               / spriteRenderer.sprite.rect.size +
			                           pos * spriteRenderer.sprite.pixelsPerUnit / spriteRenderer.sprite.rect.size,
			                           spriteRenderer.sprite.pixelsPerUnit, 0, SpriteMeshType.FullRect
			                          );
			sprites.Add(sprite);
			sprite.OverrideGeometry(vertices.ToArray(), triangles.ToArray());
			renderer.sprite = sprite;

			LeanTween.delayedCall(Vector3.Distance(obj.transform.localPosition, tanukiPos) * shatterSpeed,
			                      () => { renderer.color   = Random.ColorHSV(1f, 1f, 0f, 0f, shardTintLow, 1f); });

			renderer.sortingLayerID = SortingLayer.NameToID("DeathShatter");

			var dir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
			var ang = Random.Range(-1f, 1f);
			rigidbody.AddForce(dir * shatterForce, ForceMode2D.Impulse);
			rigidbody.AddTorque(ang, ForceMode2D.Impulse);

			rigidbody.simulated = false;

			LeanTween.delayedCall(Vector3.Distance(obj.transform.localPosition, tanukiPos) * shatterSpeed +
			                      shatterDelay,
			                      () => { rigidbody.simulated = true; });
		}

		spriteRenderer.enabled = false;

		GameUIManager.Instance.gameObject.SetActive(false);
		InitBackGround();
	}

	private void InitBackGround() {
		background.enabled = true;
		background.transform.localScale = new Vector3((mainCamera.orthographicSize) * mainCamera.aspect,
		                                              mainCamera.orthographicSize, 1f);
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
		if (seeds == null) return;
		foreach (var seed in seeds) {
			Gizmos.DrawSphere(transform.TransformPoint(seed), 0.1f);
		}

		Gizmos.color = Color.red;
		foreach (var point in spritePoints) {
			Gizmos.DrawSphere(transform.TransformPoint(point), 0.1f);
		}
	}
}