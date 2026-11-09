using System;
using System.Collections.Generic;
using Data.Skills;
using Logic;
using Player;
using Logic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI {
	[ExecuteAlways]
	[RequireComponent(typeof(PanelRenderer))]
	public class SkillTreeUiHandler : MonoBehaviour {
		[SerializeField] private Color color;
		[SerializeField] private float dotWidth   = 5f;
		[SerializeField] private float dotSpacing = 15f;
		[SerializeField] private float dotLength  = 2f;
		[SerializeField] private float speed      = 20f;
		[SerializeField] private float overlap    = 0.5f;

		[SerializeField] private List<SkillObject> leftSkills, rightSkills, bottomSkills;
		[SerializeField] private Texture2D         starFilled, starOutline;

		private PanelRenderer panelRenderer;
		private VisualElement overlay;
		private VisualElement tanuki;
		private VisualElement root;
		private List<Button>  leftCards, rightCards, bottomCards;
		private int           uiVersion = -1;

		private bool initialized;

		private void Awake() {
			panelRenderer = GetComponent<PanelRenderer>();
			panelRenderer.RegisterUIReloadCallback(OnUIReload);
		}

		private void Update() => overlay?.MarkDirtyRepaint();

		private void OnDestroy() => panelRenderer.UnregisterUIReloadCallback(OnUIReload);

		private void OnUIReload(PanelRenderer _, VisualElement root, int version) {
			if (root == null || root.childCount == 0 || version == uiVersion) return;

			var lists      = root.Query<TemplateContainer>("SkillUiList").ToList();
			var bottomList = root.Query<TemplateContainer>("BottomSkillList").ToList()[0];
			var tanukiWuki = root.Q("Tanuki");

			if (tanukiWuki == null || bottomList == null || lists.Count < 2) return;

			this.root   = root;
			uiVersion   = version;
			tanuki      = tanukiWuki;
			leftCards   = lists[0].Query<Button>("Button").ToList();
			rightCards  = lists[1].Query<Button>("Button").ToList();
			bottomCards = bottomList.Query<Button>("Button").ToList();

			overlay = new VisualElement { pickingMode = PickingMode.Ignore };
			overlay.StretchToParentSize();
			overlay.generateVisualContent += Draw;
			root.Insert(0, overlay);

			initialized = true;
			SetupSkills();
		}

		private void SetupSkills() {
			if (!initialized) return;
			SetupCards(leftCards,   leftSkills);
			SetupCards(rightCards,  rightSkills);
			SetupCards(bottomCards, bottomSkills);
			Lib.UI.UpdateExpUI(root);

			root.Q<Button>("MainMenuButton").clickable =
				new Clickable(() => { SceneTransition.Instance.TransitionToScene("MainMenu"); });
		}

		private void SetupCards(List<Button> cards, List<SkillObject> skills) {
			for (var i = 0; i < cards.Count; i++) {
				var card = cards[i];

				if (i >= skills.Count || skills[i].levelStatModifiers.Length == 0) {
					card.style.display = DisplayStyle.None;
					continue;
				}

				card.style.display = DisplayStyle.Flex;

				var skill      = skills[i];
				var mods       = skill.levelStatModifiers;
				var skillLevel = Progression.SkillLevel(skill.name);

				card.Q<Label>("SkillName").text        = skill.skillName;
				card.Q<Label>("SkillDescription").text = skill.skillDescription;

				var arrow       = card.Q<Label>("Arrow");
				var beforeValue = card.Q<Label>("BeforeValue");
				var afterValue  = card.Q<Label>("AfterValue");

				var hasBefore = skillLevel > 0;
				var hasAfter  = skillLevel < mods.Length;

				beforeValue.style.display = hasBefore ? DisplayStyle.Flex : DisplayStyle.None;
				arrow.style.display       = hasBefore && hasAfter ? DisplayStyle.Flex : DisplayStyle.None;
				afterValue.style.display  = hasAfter ? DisplayStyle.Flex : DisplayStyle.None;

				if (hasBefore) beforeValue.text = FormatStat(mods[skillLevel - 1], skill.statModifierMetricName);
				if (hasAfter) afterValue.text   = FormatStat(mods[skillLevel],     skill.statModifierMetricName);

				var stars = card.Query<Image>("Star").ToList();
				for (var starIndex = 0; starIndex < stars.Count; starIndex++) {
					var star = stars[starIndex];
					star.style.display = starIndex < mods.Length ? DisplayStyle.Flex : DisplayStyle.None;
					star.image         = starIndex < skillLevel ? starFilled : starOutline;
				}

				card.clickable = new Clickable(() => {
					                               if (Progression.TryUnlockSkill(skill.name, mods.Length))
						                               SetupSkills();
				                               });
			}
		}

		private static string FormatStat(StatModifier mod, string metric) =>
			mod.percent > 0f ? $"+{mod.percent}% {metric}" : $"+{mod.flat} {metric}";


		private void Draw(MeshGenerationContext ctx) {
			var painter    = ctx.painter2D;
			var tanukiRect = tanuki.worldBound;

			painter.strokeColor = color;
			painter.lineWidth   = dotWidth;
			painter.lineCap     = LineCap.Round;

			DrawLines(painter, leftCards, leftSkills, rect => new Vector2(rect.xMax, rect.center.y),
			          tanukiRect.center);
			DrawLines(painter, rightCards, rightSkills, rect => new Vector2(rect.xMin, rect.center.y),
			          tanukiRect.center);
			DrawLines(painter, bottomCards, bottomSkills, rect => new Vector2(rect.center.x, rect.yMin),
			          tanukiRect.center);
		}

		private void DrawLines(Painter2D           painter,    List<Button> cards, List<SkillObject> skills,
		                       Func<Rect, Vector2> cardAnchor, Vector2      tanukiAnchor) {
			for (var i = 0; i < cards.Count && i < skills.Count; i++) {
				var skill = skills[i];
				if (!skill.drawLine) continue;
				var end = tanukiAnchor + skill.lineOffset;
				DrawLine(painter, cardAnchor(cards[i].worldBound), end);
			}
		}

		private void DrawLine(Painter2D painter, Vector2 posA, Vector2 posB) {
			var d = posB - posA;
			posA += d * overlap;
			posB -= d * overlap;
			var a = overlay.WorldToLocal(posA);
			var b = overlay.WorldToLocal(posB);

			var length    = Vector2.Distance(a, b);
			var direction = (b - a)                   / length;
			var offset    = Time.unscaledTime * speed % (dotSpacing);

			painter.BeginPath();

			for (var i = offset; i < length; i += dotSpacing) {
				painter.MoveTo(a + direction * i);
				painter.LineTo(a + direction * (i + dotLength));
			}

			painter.Stroke();
		}

		#if UNITY_EDITOR
		[ContextMenu("Reset Skills")]
		public void ResetSkills() {
			SaveSystem.Data.unlockedSkills.Clear();
			SaveSystem.Save();
		}
		#endif
	}
}