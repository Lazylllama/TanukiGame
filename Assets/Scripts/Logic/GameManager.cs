using System;
using Player;
using RNG;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Logic {
	public class GameManager : MonoBehaviour {
		public static GameManager Instance;
		public        PlayerStats PlayerStats;
		public        RngHandler  RngHandler;

		public float MaxHearts      => PlayerStats.Get(Stat.ExtraHearts, defaultHearts);
		public bool  IsInvulnerable => invulnerabilityTimer > 0;

		[SerializeField] private RngTables rngTables;
		[SerializeField] public  int       defaultHearts           = 3;
		[SerializeField] public  float     invulnerabilityDuration = 0.5f;

		// state
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
			if (invulnerabilityTimer > 0) {
				invulnerabilityTimer -= Time.fixedDeltaTime;
			}
		}

		#endregion

		#region Functions

		//? game state
		private void LooseGame() {
			PlayerStats  = new PlayerStats();
			playerHealth = MaxHearts;

			KeepOnLoad.DestroyAll();
			SceneManager.LoadScene("GameScene");
			PlayerController.Instance.transform.position = new Vector3(0f, 1f, 0f);
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
		public void LoadFirstRoom() {
			SceneManager.LoadScene("RoomScene");
		}

		public void FinishRoom() {
			GameUIManager.Instance.ShowCardUI();
		}

		#endregion

		//? handlers
		private void OnStatsChangedHandler(Stat stat) {
			switch (stat) {
				case Stat.ExtraHearts: {
					var extraHearts = PlayerStats.Get(Stat.ExtraHearts, 0f);
					//? If they somehow lose the extra heart, lower the health
					if (playerHealth > MaxHearts)
						playerHealth = MaxHearts;

					//? If they gain the extra heart, increase the health
					//! this code is broken, if you lose the stat this would still give you a heart
					//! will implement like a check for this or sum sum
					playerHealth = Mathf.Min(playerHealth + 1, MaxHearts);

					GameUIManager.Instance.UpdateHeartsUI();
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