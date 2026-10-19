using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KeepOnLoad : MonoBehaviour {
	[SerializeField] private bool checkForDuplicates;

	private static readonly List<GameObject> Kept = new();

	private void Awake() {
		DontDestroyOnLoad(gameObject);

		if (checkForDuplicates) Kept.Add(gameObject);
	}

	public static void DestroyAll() {
		foreach (var gameObject in Kept.Where(gameObject => gameObject)) Destroy(gameObject);
		Kept.Clear();
	}
}