using System;
using UnityEngine;

namespace Enemy {
	public enum EnemyType {
		LanternEnemy,
		NormalEnemy
	}

	[Serializable]
	public struct Enemy {
		public EnemyType  type;
		public GameObject prefab;
		public int        health;
		public int        damageMultiplier;
		public int        speedMultiplier;
	}

	[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
	public class EnemyData : ScriptableObject {
		[SerializeField] private Enemy[] enemies;

		public Enemy[] Enemies => enemies;

		public Enemy GetEnemyByType(EnemyType type) {
			foreach (var enemy in enemies) {
				if (enemy.type == type) return enemy;
			}

			throw new Exception($"Enemy of type {type} not found.");
		}

		public GameObject GetEnemyPrefab(EnemyType type) {
			foreach (var enemy in enemies) {
				if (enemy.type == type) return enemy.prefab;
			}

			throw new Exception($"Enemy prefab of type {type} not found.");
		}
	}
}