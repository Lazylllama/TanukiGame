using Player;
using UnityEngine;

public class LanternFireBall : MonoBehaviour {
	[Header("Fireball Settings")]
	[SerializeField] private float damage;
	[SerializeField] private float knockbackForce;
	[SerializeField] private float knockbackLength;

	private void OnTriggerEnter2D(Collider2D player) {
		if (player.gameObject.layer != LayerMask.NameToLayer("Player")) return;
		Debug.Log("Player hit with fireball");

		var knockbackDirection = Mathf.Sign(player.transform.position.x - transform.position.x);
		FindAnyObjectByType<PlayerController>()
			.StartCoroutine(Lib.Combat.PreformedKnockback(player.GetComponent<Rigidbody2D>(), player.gameObject,
			                                              knockbackDirection,
			                                              knockbackForce, knockbackLength));
		Destroy(gameObject);
	}
}