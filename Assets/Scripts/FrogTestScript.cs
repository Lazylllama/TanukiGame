using System;
using Logic;
using UnityEngine;

public class FrogTestScript : MonoBehaviour {
	private void OnTriggerEnter2D(Collider2D other) {
		GameManager.Instance.FinishRoom();
	}
}