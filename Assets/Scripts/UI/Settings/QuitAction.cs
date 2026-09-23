using UnityEngine;

namespace UI.Settings {
	[CreateAssetMenu(fileName = "QuitAction", menuName = "Scriptable Objects/QuitAction")]
	class QuitAction : ActionSetting
	{
		public override void Step(bool forward = true, int set = -1) {
			Application.Quit();
			Debug.Log("Quit");
		}
	}
}
