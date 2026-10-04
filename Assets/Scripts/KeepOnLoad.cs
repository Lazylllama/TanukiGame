using System;
using UnityEngine;

public class KeepOnLoad : MonoBehaviour {
	private void Awake() => DontDestroyOnLoad(gameObject);
}