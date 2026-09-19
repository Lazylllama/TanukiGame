using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI {
	[RequireComponent(typeof(PanelRenderer))]
	public class MainMenuUiController : MonoBehaviour {
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

			var container = optionsScreen.Query<TemplateContainer>().ToList();
			for(var i = 0; i < container.Count; i++)
				container[i].Q<Label>("Name").text = i.ToString();
		
			ShowScreen(startScreen);
		}

		private void ShowScreen(VisualElement screen) {
			startScreen.EnableInClassList("screen--hidden", screen   != startScreen);
			optionsScreen.EnableInClassList("screen--hidden", screen != optionsScreen);
		}

		#endregion

		#region Functions

		#endregion

		#region Actions

		private void OnStart()   => print("OnStart");
		private void OnOptions() => ShowScreen(optionsScreen);
		private void OnBack()    => ShowScreen(startScreen);
		private void OnExit()    => Application.Quit();

		#endregion
	}
}

