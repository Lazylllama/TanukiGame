using System;
using Player;
using RNG;
using Room;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Logic {
	public class GameManager : MonoBehaviour {
		public static GameManager Instance;
		public        PlayerStats PlayerStats;
		public        RngHandler  RngHandler;

		public  float MaxHearts      => PlayerStats.Get(Stat.ExtraHearts, defaultHearts);
		private bool  IsInvulnerable => invulnerabilityTimer > 0;

		[SerializeField] private RngTables rngTables;
		[SerializeField] public  int       defaultHearts           = 3;
		[SerializeField] public  float     invulnerabilityDuration = 0.5f;
		[SerializeField] private int       xpPerEnemyHealth        = 10;

		// state
		public  bool  inRoom;
		public  float playerHealth;
		private float invulnerabilityTimer;

		#region Unity Functions

		private void Awake() {
			if (Instance != null && Instance != this) {
				Destroy(gameObject);
				return;
			}

			Instance = this;

			PlayerStats = new PlayerStats();
			RngHandler  = new RngHandler(rngTables);
		}

		private void Start() {
			PlayerStats.OnStatsChanged += OnStatsChangedHandler;

			playerHealth = MaxHearts;
			GameUIManager.Instance.UpdateHeartsUI();
		}

		private void FixedUpdate() {
			if (invulnerabilityTimer > 0)
				invulnerabilityTimer -= Time.fixedDeltaTime;
		}

		#endregion

		#region Functions

		//? game state
		private void LooseGame() {
			SceneTransition.Instance.TransitionToScene("PreGameScene",
			                                           () => {
				                                           PlayerStats  = new PlayerStats();
				                                           playerHealth = MaxHearts;

				                                           KeepOnLoad.DestroyAll();
				                                           PlayerController.Instance.transform.position =
					                                           new Vector3(0f, 1f, 0f);
			                                           });
		}

		//? player health
		public void DamagePlayer(float damage) {
			if (IsInvulnerable) return;
			playerHealth -= damage;

			GameUIManager.Instance.UpdateHeartsUI();

			invulnerabilityTimer = invulnerabilityDuration;
			if (playerHealth <= 0) LooseGame();
		}

		//? rooms
		public void LoadRoom() {
			SceneTransition.Instance.TransitionToScene("RoomScene", () => { inRoom = true; });
		}

		public void FinishRoom() {
			inRoom = false;
			GameUIManager.Instance.ShowCardUI();
		}

		#endregion

		//? handlers
		public void OnEnemyDeath(float enemyMaxHealth) {
			Progression.AddXp(Mathf.RoundToInt(PlayerStats.Get(Stat.XpGain, xpPerEnemyHealth * enemyMaxHealth)));
			if (RoomController.Instance) RoomController.Instance.OnEnemyDeath();
		}

		private void OnStatsChangedHandler(Stat stat) {
			switch (stat) {
				case Stat.ExtraHearts: {
					//? If they gain the extra heart, increase the health
					playerHealth = Mathf.Min(playerHealth + 1, MaxHearts);

					GameUIManager.Instance.UpdateHeartsUI();
				}
					break;
				case Stat.HealthBoost: {
					playerHealth = Mathf.Min(playerHealth + 0.5f, MaxHearts);
				}
					break;
				case Stat.XpGain:
				case Stat.Luck:
				case Stat.CritChance:
				case Stat.MoveSpeed:
				case Stat.ThrowDamage:
				case Stat.MeleeDamage:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(stat), stat, null);
			}
		}
	}
}