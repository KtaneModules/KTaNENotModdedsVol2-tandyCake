using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NotTextField
{
	public class Button : MonoBehaviour 
	{
		public TextMesh label;
		public KMSelectable selectable;
		public char displayedLetter;
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

}
