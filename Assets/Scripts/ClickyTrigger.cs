using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class ClickyTrigger : MonoBehaviour
{
	public string[] DialogueSequence;
	public bool SayOnce;
    public bool Done;

	File file;
	bool wasSelected;
	int nextDialogue;
	bool inDialogue;

	void Start()
	{
		file = GetComponent<File>();
	}

	void Update()
	{
	    if (SplashScreen.Instance)
	        return;

        if ((!wasSelected && file.Selected) && (!SayOnce || !Done))
		{
			inDialogue = true;
			wasSelected = true;
			Clicky.Instance.Popup(DialogueSequence[0]);
		    Clicky.Instance.Dismissed += () => ShowNext(this);

			nextDialogue = 1;
		}

		if (wasSelected && !file.Selected)
		{
			wasSelected = false;
			if (inDialogue &&
                (PlayerControl.Instance.SelectedFile.gameObject.GetComponent<ClickyTrigger>() == null ||
                 PlayerControl.Instance.SelectedFile.gameObject.GetComponent<ClickyTrigger>().Done))
			{
				Clicky.Instance.Dismiss();
				inDialogue = false;
			}
		}
	}

	void ShowNext(ClickyTrigger forTrigger)
	{
	    if (forTrigger != this)
	        return;

		if (!file.Selected)
		{
			inDialogue = false;
			return;
		}

		if (nextDialogue < DialogueSequence.Length)
		{
			//Debug.Log("Showing next : " + nextDialogue);
			Clicky.Instance.Popup(DialogueSequence[nextDialogue], 0.05f);
			Clicky.Instance.Dismissed += () => ShowNext(this);
			nextDialogue++;
		}
		else
		{
            Done = true;
			inDialogue = false;
		}
	}
}
