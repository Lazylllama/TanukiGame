using Player;
using RNG;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Logic {
	public class GameManager : MonoBehaviour {
		public static GameManager Instance;
		public        PlayerStats PlayerStats;
		public        RngHandler  RngHandler;

		[SerializeField] private RngTables rngTables;
		[SerializeField] public  int       defaultHearts = 3;
		public                   float     playerHearts;

		private void Awake() {
			if (Instance != null && Instance != this) {
				Destroy(gameObject);
				return;
			}

			Instance = this;
		}

		private void Start() {
			playerHearts = 3;

			PlayerStats = new PlayerStats();
			RngHandler  = new RngHandler(rngTables);

			PlayerStats.OnStatsChanged += OnStatsChangedHandler;
		}

		private void FixedUpdate() {
			GameUIManager.Instance.UpdateHeartsUI();
		}

		public void LoadFirstRoom() {
			SceneManager.LoadScene("RoomScene");
		}

		public void FinishRoom() {
			GameUIManager.Instance.ShowCardUI();
		}

		private void OnStatsChangedHandler(Stat stat) {
			if (stat == Stat.ExtraHearts) {
				var extraHearts = PlayerStats.Get(Stat.ExtraHearts, 0f);
				//? If they lose the extra heart, lower the health
				if (playerHearts > defaultHearts && extraHearts <= 0f) {
					playerHearts = defaultHearts;
				}

				//? If they gain the extra heart, increase the health
				playerHearts = Mathf.Clamp(playerHearts + 1, 0, defaultHearts + extraHearts);

				GameUIManager.Instance.UpdateHeartsUI();
			}
		}
	}
}