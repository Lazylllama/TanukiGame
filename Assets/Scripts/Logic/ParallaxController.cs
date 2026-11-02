// https://github.com/Lazylllama/FlashlightGame/blob/main/Assets/Scripts/ParallaxController.cs

using System.Collections.Generic;
using UnityEngine;

namespace Logic {
	[System.Serializable]
	public class ParallaxLayer {
		//? Settings
		[Range(0, 1)] public float speedMultiplier;
		[Range(0, 1)] public float speedMultiplierY;
		public               bool  infiniteHorizontal = true;
		public               bool  infiniteVertical   = false;

		//? Layer
		public Transform transform;

		//? Refs
		[HideInInspector] public List<Transform> tiles = new List<Transform>();
		[HideInInspector] public float           textureSizeX;
		[HideInInspector] public float           textureSizeY;
	}


	public class ParallaxController : MonoBehaviour {
		#region Fields

		[Header("Camera")]
		[SerializeField] GameObject playerCam;

		[Header("Layers")]
		[SerializeField] private ParallaxLayer[] layers;

		[Header("Settings")]
		[SerializeField] private float offsetX = 0f;

		//? States
		private Vector3 prevCamPos;

		#endregion

		#region Unity Functions

		private void Start() {
			if (!playerCam) {
				Debug.LogError("Camera not assigned to ParallaxController.");
				enabled = false;
				return;
			}

			prevCamPos = playerCam.transform.position;

			foreach (var layer in layers) {
				layer.tiles.Clear();

				foreach (Transform child in layer.transform) layer.tiles.Add(child);
				if (layer.tiles.Count <= 0) continue;

				var spriteRenderer = layer.tiles[0].GetComponentInChildren<SpriteRenderer>();

				if (spriteRenderer == null) {
					Debug.LogWarning($"Tile '{layer.tiles[0].name}' in layer '{layer.transform.name}' is missing a SpriteRenderer.");
					return;
				}

				layer.textureSizeX = spriteRenderer.bounds.size.x;
				layer.textureSizeY = spriteRenderer.bounds.size.y;
			}

			if (Mathf.Abs(offsetX) > 0f) {
				foreach (var layer in layers) {
					if (layer.transform != null) layer.transform.position += new Vector3(offsetX, 0f, 0f);
				}
			}
		}

		private void LateUpdate() {
			var delta = playerCam.transform.position - prevCamPos;

			foreach (var layer in layers) {
				var mov = new Vector3(delta.x * layer.speedMultiplier, delta.y * layer.speedMultiplierY, 0);

				layer.transform.position += mov;

				if (layer.infiniteHorizontal && layer.textureSizeX > 0f) {
					//? Avoid problems...
					var minX = float.MaxValue;
					var maxX = float.MinValue;
					foreach (var t in layer.tiles) {
						minX = Mathf.Min(minX, t.position.x);
						maxX = Mathf.Max(maxX, t.position.x);
					}

					var totalWidth = (maxX - minX) + layer.textureSizeX;
					if (totalWidth <= 0f) continue;

					foreach (var tile in layer.tiles) {
						var distX = tile.position.x - playerCam.transform.position.x;

						if (distX <= -totalWidth / 2f) {
							tile.position += new Vector3(totalWidth, 0, 0);
						} else if (distX >= totalWidth / 2f) {
							tile.position += new Vector3(-totalWidth, 0, 0);
						}
					}
				}
			}

			prevCamPos = playerCam.transform.position;
		}

		#endregion
	}
}