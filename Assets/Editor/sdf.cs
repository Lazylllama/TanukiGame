using System.IO;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public static class sdf
{
	[MenuItem("Tools/GenerateSDF")]
	static void generate() {
		int R = 180;
		
		var tex = Selection.activeObject as Texture2D;
		if (tex == null) return;
		var col = tex.GetPixels();
		var distances = new float[col.Length];
		var width = tex.width;
		var height = tex.height;
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				var index = y * width + x;
				float minDist = (float)R;
				if (col[index].a == 0) { distances[index] = 0; continue; }

				for (int dx = -R; dx <= R; dx++) {
					for (int dy = -R; dy <= R; dy++) {
						var nx = x + dx;
						var ny = y + dy;
						if(ny < 0 || ny >= height) continue;
						if (nx < 0) nx += width;
						else if (nx >= width) nx -= width;
						var sampleIndex = ny * width + nx;
						if (col[sampleIndex].a == 0) {
							var dist                    = Vector2.Distance(new Vector2(0, 0), new Vector2(dx, dy));
							if (dist < minDist) minDist = dist;
						}
					}
				}
				distances[index] = minDist / (float)R;
			}
		}
		
		var newTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				var index = y * width + x;
				var dist  = distances[index];
				newTex.SetPixel(x, y, new Color(dist, dist, dist, 1.0f));
			}
		}
		newTex.Apply();
		var path    = AssetDatabase.GetAssetPath(tex);
		var replace = path.Replace(".png", "_sdf.png");
		File.WriteAllBytes(replace, newTex.EncodeToPNG());
		AssetDatabase.Refresh();
	}
}
