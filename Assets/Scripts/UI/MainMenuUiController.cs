using System.Collections.Generic;
using UI.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace UI {
	[RequireComponent(typeof(PanelRenderer))]
	public class MainMenuUiController : MonoBehaviour {
		[SerializeField] private SettingsPage    settingsPage;
		[SerializeField] private VisualTreeAsset optionRow;

		private PanelRenderer panel;
		private VisualElement startScreen;
		private VisualElement optionsScreen;

		#region Initialization

		private void OnEnable() {
			panel = GetComponent<PanelRenderer>();
			panel.RegisterUIReloadCallback(OnUIReload);
		}

		private void OnDisable() {
			panel.UnregisterUIReloadCallback(OnUIReload);
		}

		private void OnUIReload(PanelRenderer panelRenderer, VisualElement root) {
			startScreen   = root.Q("StartScreen");
			optionsScreen = root.Q("OptionsScreen");

			root.Q<Button>("StartButton").clickable.clicked   += OnStart;
			root.Q<Button>("OptionsButton").clickable.clicked += OnOptions;
			root.Q<Button>("BackButton").clickable.clicked    += OnBack;
			root.Q<Button>("ExitButton").clickable.clicked    += OnExit;

			var optionsContainer = optionsScreen.Q<VisualElement>("Options");
			optionsContainer.Clear();
			foreach (var setting in settingsPage.settings) {
				var row = optionRow.Instantiate();
				row.Q<VisualElement>("OptionRow").RegisterCallback<ClickEvent>(e => {
					                                                               setting.Step();
					                                                               row.Q<Label>("Value").text =
						                                                               setting.GetDisplayValue();
				                                                               });
				row.Q<Label>("Name").text        = setting.label;
				row.Q<Label>("Description").text = setting.description;
				row.Q<Label>("Value").text       = setting.GetDisplayValue();
				optionsContainer.Add(row);
			}

			ShowScreen(startScreen);
		}

		private void ShowScreen(VisualElement screen) {
			startScreen.EnableInClassList("screen--hidden", screen   != startScreen);
			optionsScreen.EnableInClassList("screen--hidden", screen != optionsScreen);
		}

		#endregion

		#region Functions

		private void HandleOnStart() {
			SceneManager.LoadScene("GameScene");
		}

		#endregion

		#region Actions

		private void OnStart()   => HandleOnStart();
		private void OnOptions() => ShowScreen(optionsScreen);
		private void OnBack()    => ShowScreen(startScreen);
		private void OnExit()    => Application.Quit();

		#endregion
	}
}