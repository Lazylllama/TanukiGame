using Player;
using UnityEngine;

namespace Data.Skills {
	[CreateAssetMenu(fileName = "SkillObject", menuName = "Scriptable Objects/SkillObject")]
	public class SkillObject : ScriptableObject {
		public string skillName;
		public string skillDescription;

		public StatModifier[] levelStatModifiers;
		public string         statModifierMetricName;

		public bool    drawLine;
		public Vector2 lineOffset;
	}
}