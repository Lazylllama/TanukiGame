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
		[SerializeField] private VisualTreeAsset cycleControl;
		[SerializeField] private VisualTreeAsset sliderControl;

		private PanelRenderer     panel;
		private TemplateContainer startScreen;
		private TemplateContainer optionsScreen;

		#region Initialization

		private void OnEnable() {
			panel = GetComponent<PanelRenderer>();
			panel.RegisterUIReloadCallback(OnUIReload);
		}

		private void OnDisable() {
			panel.UnregisterUIReloadCallback(OnUIReload);
		}

		private void OnUIReload(PanelRenderer panelRenderer, VisualElement root) {
			startScreen   = root.Q<TemplateContainer>("StartScreen");
			optionsScreen = root.Q<TemplateContainer>("OptionsScreen");

			root.Q<Button>("StartButton").clickable.clicked   += OnStart;
			root.Q<Button>("OptionsButton").clickable.clicked += OnOptions;
			root.Q<Button>("BackButton").clickable.clicked    += OnBack;
			root.Q<Button>("ExitButton").clickable.clicked    += OnExit;

			var optionsContainer = optionsScreen.Q<VisualElement>("Options");
			optionsContainer.Clear();
			foreach (var setting in settingsPage.settings) {
				var row = optionRow.Instantiate();

				row.Q<Label>("Label").text       = setting.label;
				row.Q<Label>("Description").text = setting.description;
				var ctrlContainer = row.Q<VisualElement>("Control");
				ctrlContainer.Clear();
				if (setting is CycleSettings) {
					var ctrl     = cycleControl.Instantiate();
					var ctrlText = ctrl.Q<Label>();

					void Refresh() {
						var v = setting.GetDisplayValue();
						ctrlText.EnableInClassList("value-enabled",  v == "enabled");
						ctrlText.EnableInClassList("value-disabled", v == "disabled");
						ctrlText.text = v;
					}

					Refresh();

					row.Q<VisualElement>("OptionRow").RegisterCallback<ClickEvent>(e => {
						setting.Step();
						Refresh();
					});

					ctrl.Q<VisualElement>("LeftArrow").RegisterCallback<ClickEvent>(e => {
						setting.Step(false);
						Refresh();
						e.StopPropagation();
					});
					ctrlContainer.Add(ctrl);
				} else if (setting is SliderSettings) {
					var ctrl = sliderControl.Instantiate();
					var ctrlText = ctrl.Q<Label>();
					var ctrlSlider = ctrl.Q<Slider>();

					void Refresh() {
						var v = setting.GetDisplayValue();
						ctrlText.text = v;
					}

					Refresh();
					
					ctrlSlider.RegisterValueChangedCallback(e => {
						setting.Step(e.newValue > e.previousValue);
						Refresh();
					});
					
					ctrlContainer.Add(ctrl);
				}
				optionsContainer.Add(row);
			}
			ShowScreen(startScreen);
		}

		private void ShowScreen(TemplateContainer screen) {
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