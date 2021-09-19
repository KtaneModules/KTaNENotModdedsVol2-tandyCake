using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NTFButton : ButtonScript 
{
	public TextMesh label;
	[HideInInspector]
	public char displayedLetter;
	[HideInInspector]
	public bool submitted;

	public void SetChar(char ch)
	{
		displayedLetter = ch;
		UpdateDisp();
	}
	public void UpdateDisp()
	{
		label.text = displayedLetter.ToString();
	}
}
