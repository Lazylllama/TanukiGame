using UnityEngine;

[ExecuteAlways]
public class PlayerReflectionPos : MonoBehaviour {
	private static readonly int PlayerX = Shader.PropertyToID("_PlayerX");
	private static readonly int PlayerY = Shader.PropertyToID("_PlayerY");

	private void LateUpdate() {
		Shader.SetGlobalFloat(PlayerX, transform.position.x);
		Shader.SetGlobalFloat(PlayerY, transform.position.y);
	}
}