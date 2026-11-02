using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI {
	public class InfiniteScroll : MonoBehaviour {
		[SerializeField] private float               scrollSpeed = 100f;
		[SerializeField] private List<RectTransform> scrollElements;
		[SerializeField] private Vector2             scrollDirection = Vector2.left;

		private void FixedUpdate() {
			foreach (var scrollElement in scrollElements) {
				var newPosition = scrollElement.anchoredPosition;
				newPosition.x += scrollDirection.x * (scrollSpeed * Time.deltaTime);

				if (newPosition.x < -scrollElement.rect.width) {
					newPosition.x += scrollElement.rect.width * scrollElements.Count;
				}

				scrollElement.anchoredPosition = newPosition;
			}
		}
	}
}