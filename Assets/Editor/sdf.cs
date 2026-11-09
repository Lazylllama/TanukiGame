using System;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public static class sdf {
	[MenuItem("Tools/GenerateSDF")]
	static void generate() {
		int R = 180;

		var tex = Selection.activeObject as Texture2D;
		if (tex == null) return;
		var col          = tex.GetPixels();
		var distances    = new float[col.Length];
		var rawDistances = new float[col.Length];
		var width        = tex.width;
		var height       = tex.height;
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				var   index   = y * width + x;
				float minDist = (float)R;
				if (col[index].a == 0) {
					distances[index]    = 0;
					rawDistances[index] = 0;
					continue;
				}

				for (int dx = -R; dx <= R; dx++) {
					for (int dy = -R; dy <= R; dy++) {
						var nx = x + dx;
						var ny = y + dy;
						if (ny < 0 || ny >= height) continue;
						if (nx      < 0) nx      += width;
						else if (nx >= width) nx -= width;
						var sampleIndex = ny * width + nx;
						if (col[sampleIndex].a == 0) {
							var dist                    = Vector2.Distance(new Vector2(0, 0), new Vector2(dx, dy));
							if (dist < minDist) minDist = dist;
						}
					}
				}

				distances[index]    = minDist / (float)R;
				rawDistances[index] = minDist;
			}
		}


		var radii    = new int[3] { 20, 10, 5 };
		var minInset = new int[3] { 12, 5, 1 };
		var maxInset = new int[3] { 26, 12, 5 };
		var gaps     = new int[3] { 26, 13, 7 };
		var indexes  = new int[3];

		var random      = new System.Random(1234);
		var domeHeights = new float[col.Length];

		for (int size = 0; size < 3; size++) {
			int currentIndex = 0;
			var candidates   = new int[col.Length];

			for (var index = 0; index < candidates.Length; index++) {
				if (rawDistances[index] < minInset[size] || rawDistances[index] > maxInset[size]) continue;
				candidates[currentIndex] = index;
				currentIndex++;
			}


			for (var index = currentIndex - 1; index >= 0; index--) {
				var randomIndex = random.Next(0, index + 1);
				(candidates[randomIndex], candidates[index]) = (candidates[index], candidates[randomIndex]);
			}


			List<int> accepted = new List<int>();
			for (var index = 0; index < currentIndex; index++) {
				var pos = new Vector2(Mathf.Repeat(candidates[index], width),
				                      Mathf.FloorToInt(candidates[index] / (float)width));
				var tooClose = false;
				foreach (var compare in accepted) {
					var dx = Mathf.Abs(Mathf.Repeat(compare, width) - pos.x);
					dx = Mathf.Min(dx, width - dx);
					var comparePos = new Vector2(dx, Mathf.FloorToInt(compare / (float)width));
					if (Mathf.Pow(comparePos.x, 2f) + Mathf.Pow(comparePos.y - pos.y, 2f) < Mathf.Pow(gaps[size], 2f))
						tooClose = true;
				}

				if (!tooClose) accepted.Add(candidates[index]);
			}


			foreach (var index in accepted) {
				var pos = new Vector2(Mathf.Repeat(index, width),
				                      Mathf.FloorToInt(index / (float)width));
				for (int dx = -radii[size]; dx <= radii[size]; dx++) {
					for (int dy = -radii[size]; dy <= radii[size]; dy++) {
						var newPos = new Vector2(Mathf.Repeat(pos.x + dx, width), pos.y + dy);
						if (newPos.y < 0 || newPos.y >= height) continue;
						var t = Mathf.Pow(Mathf.Abs(dx), 2f) + Mathf.Pow(Mathf.Abs(dy), 2f);
						if (t < Mathf.Pow(radii[size], 2f))
							domeHeights[(int)newPos.y * width + (int)newPos.x] =
								Mathf.Max(Mathf.Sqrt(Mathf.Pow(radii[size], 2f) - t),
								          domeHeights[(int)newPos.y * width     + (int)newPos.x]);
					}
				}
			}
		}

		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				var currentIndex = y * width + x;
				if (col[currentIndex].a == 0) {
					domeHeights[currentIndex] = 0;
					continue;
				}
				domeHeights[currentIndex] *= Mathf.Clamp(1 - rawDistances[currentIndex] / 40, 0, 1);
			}
		}

		var newTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				var index = y * width + x;
				var dist  = distances[index];
				newTex.SetPixel(x, y, new Color(dist, domeHeights[index] / 20, dist, 1.0f));
			}
		}

		newTex.Apply();
		var path    = AssetDatabase.GetAssetPath(tex);
		var replace = path.Replace(".png", "_sdf.png");
		File.WriteAllBytes(replace, newTex.EncodeToPNG());
		AssetDatabase.Refresh();
	}
}