using UnityEngine;

[ExecuteAlways]
public class PlayerReflectionPos : MonoBehaviour
{
	void LateUpdate() => Shader.SetGlobalFloat("_PlayerX", transform.position.x);
}