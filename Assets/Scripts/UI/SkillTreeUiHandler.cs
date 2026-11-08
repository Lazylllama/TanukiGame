using System;
using System.Collections.Generic;
using Data.Skills;
using Player;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

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

		private PanelRenderer panelRenderer;
		private VisualElement overlay;
		private VisualElement tanuki;
		private List<Button>  leftCards, rightCards, bottomCards;
		private int           uiVersion = -1;

		private bool initialized;

		private void Awake() {
			panelRenderer = GetComponent<PanelRenderer>();
			panelRenderer.RegisterUIReloadCallback(OnUIReload);
		}

		private void Start() {
			SetupSkills();
		}

		private void Update()    => overlay?.MarkDirtyRepaint();
		private void OnDestroy() => panelRenderer.UnregisterUIReloadCallback(OnUIReload);

		private void OnUIReload(PanelRenderer _, VisualElement root, int version) {
			if (root == null || root.childCount == 0 || version == uiVersion) return;
			uiVersion = version;

			var lists      = root.Query<TemplateContainer>("SkillUiList").ToList();
			var bottomList = root.Query<TemplateContainer>("BottomSkillList").ToList()[0];
			var tanukiWuki = root.Q("Tanuki");

			if (tanukiWuki == null || bottomList == null || lists.Count < 2) return;

			tanuki      = tanukiWuki;
			leftCards   = lists[0].Query<Button>("Button").ToList();
			rightCards  = lists[1].Query<Button>("Button").ToList();
			bottomCards = bottomList.Query<Button>("Button").ToList();

			overlay = new VisualElement { pickingMode = PickingMode.Ignore };
			overlay.StretchToParentSize();
			overlay.generateVisualContent += Draw;
			root.Insert(0, overlay);

			initialized = true;
		}

		private void SetupSkills() {
			if (!initialized) return;
			foreach (var card in leftCards) {
				if (leftCards.IndexOf(card) >= leftSkills.Count) {
					card.style.display = DisplayStyle.None;
					continue;
				}

				var skill      = leftSkills[leftCards.IndexOf(card)];
				var skillLevel = 0;

				var beforeSkillStat = skillLevel > 0 ? skill.levelStatModifiers[skillLevel - 1] : (StatModifier?)null;
				var afterSkillStat  = skill.levelStatModifiers[skillLevel];

				var afterSkillMetric = afterSkillStat.percent > 0f
					                       ? $"% {skill.statModifierMetricName}"
					                       : $" {skill.statModifierMetricName}";

				var beforeSkillMetric = beforeSkillStat?.percent > 0f
					                        ? $"% {skill.statModifierMetricName}"
					                        : $" {skill.statModifierMetricName}";

				card.Q<Label>("SkillName").text        = skill.name;
				card.Q<Label>("SkillDescription").text = skill.skillDescription;

				var arrow       = card.Q<Label>("Arrow");
				var beforeValue = card.Q<Label>("BeforeValue");
				var afterValue  = card.Q<Label>("AfterValue");

				if (beforeSkillStat != null) {
					beforeValue.text = $"+{skill.levelStatModifiers[skillLevel - 1]}{beforeSkillMetric}";
				} else {
					beforeValue.SetEnabled(false);
					arrow.SetEnabled(false);
				}

				if (skillLevel < skill.levelStatModifiers.Length) {
					afterValue.text = $"+{afterSkillStat}{afterSkillMetric}";
				}

				afterValue.text = $"+{skill.levelStatModifiers}{skill.statModifierMetricName}";
			}
		}

		private void Draw(MeshGenerationContext ctx) {
			var painter    = ctx.painter2D;
			var tanukiRect = tanuki.worldBound;

			painter.strokeColor = color;
			painter.lineWidth   = dotWidth;
			painter.lineCap     = LineCap.Round;

			foreach (var card in leftCards) {
				var cardIndex = leftCards.IndexOf(card);
				var positionAWithOffset =
					new Vector2(card.worldBound.xMax, card.worldBound.center.y);

				var positionBWithOffset =
					leftSkills.Count > cardIndex && leftSkills[cardIndex].drawLine
						? new Vector2(tanukiRect.xMin, tanukiRect.center.y) + leftSkills[cardIndex].lineOffset
						: new Vector2(tanukiRect.xMin, tanukiRect.center.y);

				DrawDots(painter, positionAWithOffset, positionBWithOffset);
			}

			foreach (var card in rightCards) {
				var cardIndex = rightCards.IndexOf(card);
				var positionAWithOffset =
					new Vector2(card.worldBound.xMin, card.worldBound.center.y);

				var positionBWithOffset =
					rightSkills.Count > cardIndex && rightSkills[cardIndex].drawLine
						? new Vector2(tanukiRect.xMin, tanukiRect.center.y) + rightSkills[cardIndex].lineOffset
						: new Vector2(tanukiRect.xMin, tanukiRect.center.y);

				DrawDots(painter, positionAWithOffset, positionBWithOffset);
			}


			foreach (var card in bottomCards) {
				var cardIndex = bottomCards.IndexOf(card);
				var positionAWithOffset =
					new Vector2(card.worldBound.xMin, card.worldBound.center.y);

				var positionBWithOffset =
					rightSkills.Count > cardIndex && rightSkills[cardIndex].drawLine
						? new Vector2(tanukiRect.xMin, tanukiRect.center.y) + rightSkills[cardIndex].lineOffset
						: new Vector2(tanukiRect.xMin, tanukiRect.center.y);

				DrawDots(painter, positionAWithOffset, positionBWithOffset);
			}
		}

		private void DrawDots(Painter2D painter, Vector2 posA, Vector2 posB) {
			posA = new Vector2(posA.x + (posB.x - posA.x) * overlap, posA.y);
			posB = new Vector2(posB.x - (posB.x - posA.x) * overlap, posB.y);
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
	}
}