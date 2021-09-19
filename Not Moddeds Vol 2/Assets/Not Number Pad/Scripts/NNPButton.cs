using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NotNumberPad;

public class NNPButton : ButtonScript
{
	public MeshRenderer buttonMesh;
	[HideInInspector]
	public ButtonColor color;
	[HideInInspector]
	public bool isLit;
	public int? value = null;

	private NotNumberPadScript parentScript;
	private Coroutine anim;

    public override void Start()
	{
		Debug.Log("buttonmesh is null: " + buttonMesh == null);
		Debug.Log("selectable is null: " + selectable == null);

		parentScript = transform.parent.parent.parent.GetComponent<NotNumberPadScript>();
		selectable.OnInteract += MoveButton;
	}
	public bool MoveButton()
	{
		if (anim != null)
			StopCoroutine(anim);
		anim = StartCoroutine(ButtonAnimation());
		if (value == null || parentScript.initState || parentScript.displayMesh.text.Length == 4)
			parentScript.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		else 
		{
			StartCoroutine(Enable()); //Needs to wait one frame before trying to turn the button on.
			parentScript.Audio.PlaySoundAtTransform(color.ToString()[0].ToString(), transform);
		}
		return false;
	}
	public void SetState(bool state)
    {
		isLit = state;
		UpdateAppearance();	
    }
	IEnumerator Enable()
    {
		yield return null;
		SetState(true);
    }
	IEnumerator ButtonAnimation()
	{
		while (transform.localPosition.y > -0.008f)
		{
			transform.localPosition += 0.04f * Time.deltaTime * Vector3.down;
			yield return null;
		}
		while (transform.localPosition.y < -0.0044f)
		{
			transform.localPosition += 0.04f * Time.deltaTime * Vector3.up;
			yield return null;
		}
		transform.localPosition = new Vector3(transform.localPosition.x, -0.0044f, transform.localPosition.z);
	}

    public void UpdateAppearance()
	{
		buttonMesh.material = isLit ? parentScript.unlitMat : parentScript.diffuseMat;
		buttonMesh.material.color = (isLit ? Data.litButtonColors : Data.unlitButtonColors)[(int)color];
	}

    public override string ToString()
    {
		return color.ToString()[0].ToString();
    }
}
