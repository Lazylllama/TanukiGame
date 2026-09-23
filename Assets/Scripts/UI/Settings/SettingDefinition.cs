using System;
using UnityEngine;

namespace UI.Settings {
	abstract class SettingsDefinition : ScriptableObject {
		public string id;
		public string label;
		public string description;

		public abstract string GetDisplayValue();

		public virtual void Step(bool forward = true, int set = -1){}
	}
}