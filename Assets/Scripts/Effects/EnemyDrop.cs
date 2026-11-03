using Scratch;
using UnityEngine;

namespace Enemy{
	[RequireComponent(typeof(EnemyHealth))]
	public class EnemyDrop : MonoBehaviour {
		[SerializeField] private GameObject  dropPrefab;
		private                  EnemyHealth eh;

		private void Start() {
			eh              =  GetComponent<EnemyHealth>();
			eh.OnDeathEvent += DropItem;
		}

		private void OnDestroy() {
			eh.OnDeathEvent -= DropItem;
		}

		private void DropItem() {
			GameObject drop;
			if (dropPrefab) drop = Instantiate(dropPrefab);
			else return;
			drop.transform.parent = null;
			drop.transform.localPosition = gameObject.transform.localPosition;
		}
	}
}
