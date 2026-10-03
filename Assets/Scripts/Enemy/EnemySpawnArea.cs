using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemy {
	public class EnemySpawnArea : MonoBehaviour {
		[Header("General settings")]
		[SerializeField] private EnemyType enemyType;
		[SerializeField] private int   enemyCount;
		[SerializeField] private float spawnAreaWidth;

		[Header("Data")]
		[SerializeField] private EnemyData enemyData;


		private void Awake() {
			if (!enemyData) {
				Debug.LogError("EnemyData is not assigned or does not contain the specified EnemyType.");
				Destroy(gameObject);
			}
		}

		private void OnDrawGizmos() {
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(transform.position, new Vector3(spawnAreaWidth, 4, 0));
		}

		private void SpawnEnemy() {
			var spawnLocationOffsetX = Random.Range(-spawnAreaWidth / 2, spawnAreaWidth / 2);
			var spawnLocation = new Vector3(transform.position.x - spawnLocationOffsetX, transform.position.y, 0);
			var spawnedEnemy = Instantiate(enemyData.GetEnemyPrefab(enemyType),
			                               spawnLocation, Quaternion.identity);
		}

		private void Start() {
			for (var i = 0; i < enemyCount; i++)
				SpawnEnemy();
		}
	}
}