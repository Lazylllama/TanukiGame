using System;
using System.Collections.Generic;
using Unity.Mathematics;
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

		private PanelRenderer panelRenderer;
		private VisualElement overlay;
		private VisualElement tanuki;
		private List<Button>  leftCards, rightCards;
		private int           uiVersion = -1;

		private void Awake() {
			panelRenderer = GetComponent<PanelRenderer>();
			panelRenderer.RegisterUIReloadCallback(OnUIReload);
		}

		private void Update()    => overlay?.MarkDirtyRepaint();
		private void OnDestroy() => panelRenderer.UnregisterUIReloadCallback(OnUIReload);

		private void OnUIReload(PanelRenderer _, VisualElement root, int version) {
			if (root == null || root.childCount == 0 || version == uiVersion) return;
			uiVersion = version;

			var lists = root.Query<TemplateContainer>("SkillUiList").ToList();

			tanuki     = root.Q("Tanuki");
			leftCards  = lists[0].Query<Button>("Button").ToList();
			rightCards = lists[1].Query<Button>("Button").ToList();

			if (tanuki == null || lists.Count < 2) return; // make sure shit is there

			overlay = new VisualElement { pickingMode = PickingMode.Ignore };
			overlay.StretchToParentSize();
			overlay.generateVisualContent += Draw;
			root.Insert(0, overlay);
		}

		private void Draw(MeshGenerationContext ctx) {
			var painter    = ctx.painter2D;
			var tanukiRect = tanuki.worldBound;

			painter.strokeColor = color;
			painter.lineWidth   = dotWidth;
			painter.lineCap     = LineCap.Round;

			foreach (var card in leftCards) {
				DrawDots(painter, new Vector2(card.worldBound.xMax, card.worldBound.center.y),
				         new Vector2(tanukiRect.xMin,               tanukiRect.center.y));
			}

			foreach (var card in rightCards) {
				DrawDots(painter, new Vector2(card.worldBound.xMin, card.worldBound.center.y),
				         new Vector2(tanukiRect.xMax,               tanukiRect.center.y));
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