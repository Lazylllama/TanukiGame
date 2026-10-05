using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KeepOnLoad : MonoBehaviour {
	[SerializeField] private bool checkForDuplicates;

	private static readonly List<GameObject> kept = new();

	private void Awake() {
		DontDestroyOnLoad(gameObject);

		if (checkForDuplicates) kept.Add(gameObject);
	}

	public static void DestroyAll() {
		foreach (var gameObject in kept.Where(gameObject => gameObject)) Destroy(gameObject);
		kept.Clear();
	}
}