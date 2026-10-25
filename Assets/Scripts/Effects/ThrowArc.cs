using System;
using Player;
using UnityEngine;

namespace Effects {
	[RequireComponent(typeof(LineRenderer))]
	public class ThrowArc : MonoBehaviour {
		private static readonly  int       ArcLength    = Shader.PropertyToID("_ArcLength");
		private static readonly  int       LineWidth    = Shader.PropertyToID("_LineWidth");
		private static readonly  int       Charge       = Shader.PropertyToID("_Charge");
		private static readonly  int       UnscaledTime = Shader.PropertyToID("_UnscaledTime");
		[SerializeField] private int       maxSteps     = 90;
		[SerializeField] private LayerMask groundMask;

		private Rigidbody2D           throwableRb;
		private LineRenderer          line;
		private MaterialPropertyBlock materialPropertyBlock;

		private void Awake() {
			materialPropertyBlock = new MaterialPropertyBlock();

			line         = GetComponent<LineRenderer>();
			line.enabled = false;
		}

		private void Start() {
			if (!PlayerCombat.Instance) return;
			throwableRb = PlayerCombat.Instance.throwObject.GetComponent<Rigidbody2D>();
		}

		private void Update() {
			var combat = PlayerCombat.Instance;
			line.enabled = combat && combat.IsChargingThrow;
			if (!line.enabled) return;

			var arcLength = DrawArc(combat.transform.position, combat.ThrowImpulse / throwableRb.mass);

			line.GetPropertyBlock(materialPropertyBlock);
			materialPropertyBlock.SetFloat(ArcLength,    arcLength);
			materialPropertyBlock.SetFloat(LineWidth,    line.widthMultiplier);
			materialPropertyBlock.SetFloat(Charge,       combat.ThrowCharge);
			materialPropertyBlock.SetFloat(UnscaledTime, Time.unscaledTime);
			line.SetPropertyBlock(materialPropertyBlock);
		}

		private float DrawArc(Vector2 position, Vector2 velocity) {
			var gravity   = Physics2D.gravity * throwableRb.gravityScale;
			var deltaTime = Time.fixedDeltaTime;
			var length    = 0f;

			line.positionCount = maxSteps + 1;
			line.SetPosition(0, position);

			for (var i = 0; i <= maxSteps; i++) {
				velocity += gravity * deltaTime;
				var next = position + velocity * deltaTime;

				var hit       = Physics2D.Linecast(position, next, groundMask);
				if (hit) next = hit.point;

				length += Vector2.Distance(position, next);
				line.SetPosition(i, next);
				position = next;

				if (hit) {
					line.positionCount = i + 1;
					break;
				}
			}

			return length;
		}
	}
}