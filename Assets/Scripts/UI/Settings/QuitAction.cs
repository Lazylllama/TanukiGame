using UnityEngine;

namespace UI.Settings {
	[CreateAssetMenu(fileName = "QuitAction", menuName = "Scriptable Objects/QuitAction")]
	class QuitAction : ActionSetting
	{
		public override void Step(bool forward = true) {
			Application.Quit();
			Debug.Log("Quit");
		}
	}
}
