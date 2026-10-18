using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Logic;
using UI.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace UI {
	[RequireComponent(typeof(PanelRenderer))]
	public class MainMenuUiManager : MonoBehaviour {
		#region Fields

		[SerializeField] private SettingsPage    settingsPage;
		[SerializeField] private VisualTreeAsset optionRow;
		[SerializeField] private VisualTreeAsset cycleControl;
		[SerializeField] private VisualTreeAsset sliderControl;

		private PanelRenderer     panel;
		private TemplateContainer startScreen;
		private TemplateContainer optionsScreen;
		private VisualElement     optionsContainer;

		private bool isLoading;

		#endregion

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

			optionsContainer = optionsScreen.Q<VisualElement>("Options");
			optionsContainer.Clear();

			LoadOptionsScreen(settingsPage);
			ShowScreen(startScreen);
			UpdateExpUI();
		}

		private void LoadOptionsScreen(SettingsPage page) {
			foreach (var setting in page.settings) {
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
				} else if (setting is SliderSettings slider) {
					var ctrl       = sliderControl.Instantiate();
					var ctrlText   = ctrl.Q<Label>();
					var ctrlSlider = ctrl.Q<Slider>();
					ctrlSlider.highValue = slider.maxValue;

					int.TryParse(setting.GetDisplayValue(), out var outValue);
					ctrlSlider.value = outValue;

					void Refresh() {
						var v = setting.GetDisplayValue();
						ctrlText.text = v;
					}

					Refresh();

					ctrlSlider.RegisterValueChangedCallback(e => {
						                                        setting.Step(true, (int)e.newValue);
						                                        Refresh();
					                                        });

					ctrlContainer.Add(ctrl);
				}

				optionsContainer.Add(row);
			}
		}

		private void ShowScreen(TemplateContainer screen) {
			startScreen.EnableInClassList("screen--hidden", screen   != startScreen);
			optionsScreen.EnableInClassList("screen--hidden", screen != optionsScreen);
		}

		#endregion

		#region Functions

		private IEnumerator HandleOnStart() {
			if (isLoading) yield break;
			isLoading = true;
			var load = SceneManager.LoadSceneAsync($"LoadingScreen");

			if (load == null) {
				isLoading = false;
				yield break;
			}

			load.allowSceneActivation = false;
			while (load.progress < 0.9f) yield return null;
			load.allowSceneActivation = true;
			yield return load;

			isLoading = false;
		}

		private void UpdateExpUI() {
			var xp     = SaveSystem.Data.totalXp;
			var level  = Progression.Level;
			var start  = Progression.XpForLevel(level);
			var end    = Progression.XpForLevel(level + 1);
			var points = Progression.SkillPoints;

			startScreen.Q<Label>("ExpLabel").text         = $"{xp - start}/{end - start}";
			startScreen.Q<Label>("LevelLabel").text       = level.ToString();
			startScreen.Q<Label>("SkillPointsLabel").text = points > 0 ? $"{points} skill points available" : "";

			var bar = startScreen.Q<ProgressBar>("ExpBar");
			bar.lowValue  = 0;
			bar.highValue = end - start;
			bar.value     = xp  - start;
		}

		#endregion

		#region Actions

		private void OnStart()   => StartCoroutine(HandleOnStart());
		private void OnOptions() => ShowScreen(optionsScreen);
		private void OnBack()    => ShowScreen(startScreen);
		private void OnExit()    => Application.Quit();

		#endregion
	}
}