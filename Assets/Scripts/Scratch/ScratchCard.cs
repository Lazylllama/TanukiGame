using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ScratchCard : MonoBehaviour {
	#region Fields

	[SerializeField]                    private Material   scratchBrushMaterial, dissolveMaterial;
	[SerializeField]                    private GameObject crumbPrefab,          prize, parent;
	[SerializeField]                    private Canvas     canvas;
	[SerializeField] [Range(5f,  100f)] private float      crumbPerUV = 10f;
	[SerializeField] [Range(10f, 100f)] private float      revealSpeed;

	#endregion

	#region Refs

	private RenderTexture rt, readTexture, dissolveRt;
	private RectTransform rectTransform;
	private Coroutine     scratchCountRoutine;
	private Material      mat;
	private Vector2       lastLocalPos, lastMousePos;
	private Action        callOnFinish;
	private float         dissolveTimer;
	private bool          revealing;

	#endregion

	#region Unity Functions

	void Start() {
		//? Gets the material from the image component and makes copy. Then assigns the copy to the image.
		var image = GetComponent<RawImage>();
		rectTransform  = gameObject.GetComponent<RectTransform>();
		mat            = new Material(image.material);
		image.material = mat;

		//? Creates all the needed RenderTextures.
		rt = new RenderTexture(512, (int)(512f * rectTransform.rect.height / rectTransform.rect.width), 0,
		                       RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
		rt.Create();
		dissolveRt = new RenderTexture(512, (int)(512f * rectTransform.rect.height / rectTransform.rect.width), 0,
		                               RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
		dissolveRt.Create();
		readTexture = new RenderTexture(32, 32, 0,
		                                RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
		readTexture.Create();

		//? Clears the foil RenderTexture to black which is fully covered.
		var active = RenderTexture.active;
		RenderTexture.active = rt;
		GL.Clear(true, true, Color.black);
		RenderTexture.active = active;

		//? Assigns the scratch mask to the RenderTexture.
		mat.SetTexture("_MaskTex", rt);

		//? Assigns the aspect ratio of the scratch zone to the ScratchBrush so that the scratch isn't shaped like an egg.
		scratchBrushMaterial.SetFloat("_Aspect", rectTransform.rect.width / rectTransform.rect.height);
		
		//? Hide the card until needed.
		parent.SetActive(false);
	}

	void Update() {
		if (revealing) {
			dissolveTimer += Time.deltaTime;
			while (dissolveTimer >= 1f / revealSpeed) {
				dissolveTimer -= 1f / revealSpeed;

				//? Blit the texture with the dissolve material to make it dissolve.
				Graphics.Blit(rt,         dissolveRt, dissolveMaterial);
				Graphics.Blit(dissolveRt, rt);
			}

			//? Don't allow the player to scratch while it's dissolving.
			return;
		}

		//? Get the current mouse position on the screen.
		var mousePos = Mouse.current.position.ReadValue();

		//? Get the camera that the canvas uses to render.
		var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

		//? Transform from screen position to coordinates relative to the scratch zone.
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePos, cam, out var localPos);

		//? Transform to UV coordinates (0, 0) - (1, 1).
		localPos.x /= rectTransform.rect.width;
		localPos.y /= rectTransform.rect.height;
		localPos   += new Vector2(0.5f, 0.5f);

		//? Stop if the lmb isn't pressed.
		if (!Mouse.current.leftButton.isPressed) return;
		if (Mouse.current.leftButton.wasPressedThisFrame) {
			//? Reset the positions to avoid drawing a line from the last scratched point to the newest scratched point.
			lastLocalPos = localPos;
			lastMousePos = mousePos;
		}

		//? Assign values for the brush shader so it know where to draw.
		scratchBrushMaterial.SetVector("_PrevBrushUV", lastLocalPos);
		scratchBrushMaterial.SetVector("_BrushUV",     localPos);

		//? Draw the scratch to the Texture
		Graphics.Blit(Texture2D.whiteTexture, rt, scratchBrushMaterial);

		//? Get how many crumbles to produce based on the distance the mouse moved.
		var distanceMoved = Vector2.Distance(lastLocalPos, localPos);
		var crumbleAMount = Mathf.Floor(Mathf.Min(distanceMoved * crumbPerUV, 6f));

		//? Crumb spawning		
		for (int i = 0; i < crumbleAMount; i++) {
			//? Get the position on the line between the last mouse pos and current mouse pos.
			var spawnPos = lastMousePos + (((float)i / crumbleAMount) * (mousePos - lastMousePos));

			//? Convert the spawnPosition to be relative to the scratch zone.
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, spawnPos, cam,
			                                                        out var spawnLocalPos);
			spawnLocalPos.x /= rectTransform.rect.width;
			spawnLocalPos.y /= rectTransform.rect.height;
			spawnLocalPos   += new Vector2(0.5f, 0.5f);

			//? If the position is inside the scratch zone spawn it.
			if (spawnLocalPos.x is <= 1.0f and >= 0.0f && spawnLocalPos.y is <= 1.0f and >= 0.0f)
				SpawnCrumb(spawnPos, cam);
		}

		//? Set the positions for next frame.
		lastLocalPos = localPos;
		lastMousePos = mousePos;
	}

	private void OnDestroy() {
		//? Release and destroy all the RenderTextures.
		if (rt) {
			rt.Release();
			Destroy(rt);
		}

		if (dissolveRt) {
			dissolveRt.Release();
			Destroy(dissolveRt);
		}

		if (readTexture) {
			readTexture.Release();
			Destroy(readTexture);
		}

		//? Destroy the copy of the material.
		if (mat) Destroy(mat);
	}

	#endregion

	#region Custom Functions

	[ContextMenu("Reveal")]
	private void ShowScratchCard() => ShowScratchCard(() => { Debug.Log("Test"); });

	public void ShowScratchCard(Action callback) {
		//? Shows the card.
		parent.SetActive(true);
		
		//? Resets the card to make sure it isn't scratched already.
		ResetCard();
		
		if (callback != null) callOnFinish = callback; //? If an action is given assign it to callOnFinish
		if (callback == null)
			callOnFinish = () => {
				               Debug.LogWarning("A function was not given to ShowScratchCard in ScratchCard.cs");
			               }; //? If an action is not given log a warning.
	}

	[ContextMenu("Reset")]
	private void ResetCard() {
		//? Reset all the values and RenderTextures so that it can be scratched again.
		var active = RenderTexture.active;
		RenderTexture.active = rt;
		GL.Clear(true, true, Color.black);
		RenderTexture.active = readTexture;
		GL.Clear(true, true, Color.black);
		RenderTexture.active = dissolveRt;
		GL.Clear(true, true, Color.black);
		RenderTexture.active = active;
		dissolveTimer        = 0f;
		revealing            = false;
		if (scratchCountRoutine != null) StopCoroutine(scratchCountRoutine);
		scratchCountRoutine = StartCoroutine(ReadScratchAmount());
		LeanTween.cancel(prize);
	}

	private void RevealPrize() {
		//? Make the prize pop when fully revealed.
		prize.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
		LeanTween.scale(prize, new Vector3(1f, 1f, 1f), 0.4f).setEaseOutBack();
	}

	private void SpawnCrumb(Vector2 mousePos, Camera cam) {
		//? Get the position of the mouse relative to the canvas.
		RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), mousePos, cam,
		                                                        out var localPos);

		//? Spawn a new crumb.
		var crumb = Instantiate(crumbPrefab, Vector3.zero, Quaternion.identity);

		//? Set the crumbs parent to the canvas.
		crumb.transform.SetParent(canvas.transform);

		//? Set the scale to 0.2.
		crumb.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

		//? Set the position to the previously converted localPos plus a random offset so that it isn't a perfect line.
		crumb.transform.localPosition = localPos + Random.insideUnitCircle * 20f;

		//? Give the crumb a rigidbody so it falls.
		var rb = crumb.AddComponent<Rigidbody2D>();

		//? Add a slightly random force so the crumbs pop.
		rb.AddForce(Vector2.up,                            ForceMode2D.Impulse);
		rb.AddForce(Vector2.right * Random.Range(-1f, 1f), ForceMode2D.Impulse);

		//? Destroy the crumb after 1.5 seconds.
		Destroy(crumb, 1.5f);
	}

	#endregion

	#region Coroutines

	private IEnumerator ReadScratchAmount() {
		while (true) {
			//? Copy the mask texture into a lower quality texture so it can be read without taking 50 years.
			Graphics.Blit(rt, readTexture);

			//? Read the texture with Async so that it doesn't hang the game.
			var request = AsyncGPUReadback.Request(readTexture);

			//? Wait until the readback is complete.
			yield return new WaitUntil(() => request.done);

			//? Try again if the request has an error
			if (request.hasError) continue;

			//? Gets the readback data.
			var data = request.GetData<byte>();

			//? Counts the amount of bytes in the texture.
			var count = 0;
			foreach (var b in data) {
				count += b;
			}

			//? Adjusts the count to a number between 0 and 1. Where 0 is fully covered and 1 is fully scratched.
			var adjusted = count / (data.Length * 255f);

			//? If more than 60% of the prize is revealed start revealing the rest automatically.
			if (adjusted >= 0.6f) revealing = true;

			//? If more that 99% of the prize is revealed stop revealing automatically and make the prize pop. 
			if (adjusted >= 0.99f) {
				revealing = false;
				RevealPrize();

				//? If a function has been given call it after a small delay.
				LeanTween.delayedCall(1f, () => {
					                          callOnFinish?.Invoke();
					                          callOnFinish = null;
				                          });

				//? Stop the coroutine as it no longer needs to count how much is scratched.
				yield break;
			}

			//? Wait for 0.2 seconds to read again.
			yield return new WaitForSeconds(0.2f);
		}
	}

	#endregion
}