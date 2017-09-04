using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class WinFlow : MonoBehaviour
{
	public static WinFlow Instance;

	public int State;

	public bool started;

	GameObject Doggie1, Doggie2;
	GameObject DoggiePanel;

	GameObject WhichDoggie;

	void Start()
	{
		DoggiePanel = transform.Find("DoggiePanel").gameObject;
		Doggie1 = DoggiePanel.transform.Find("Doggie1").gameObject;
		Doggie2 = DoggiePanel.transform.Find("Doggie2").gameObject;

		WhichDoggie = UnityEngine.Random.value < 0.5 ? Doggie1 : Doggie2;

		foreach (var i in GetComponentsInChildren<UnityEngine.UI.Image>())
			i.color = new Color(1, 1, 1, 0.0f);

		Instance = this;
	}

	public void Win()
	{
	    var ct = PlayerControl.Instance.SelectedFile.gameObject.AddComponent<ClickyTrigger>();
        ct.DialogueSequence = new[] { "ClickyPlayerWin01" };

		State = 0;
		started = true;
		GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, 1.0f);

	    SoundRegistry.Instance.FadeOut();
	}

	void Update()
	{
		if (!started || Clicky.Instance.Visible) return;

		if (Input.GetKeyDown(KeyCode.Return))
		{
			State++;

			if (State == 1)
			{
				DoggiePanel.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, 1.0f);
				WhichDoggie.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, 1.0f);

                WhichDoggie.GetComponent<UnityEngine.UI.Image>().material.SetFloat("_StartTime", Time.timeSinceLevelLoad);

                GetComponent<AudioSource>().PlayOneShot(SoundRegistry.Instance.WinJingle);
			}
			else if (State == 2)
			{
                var ct = PlayerControl.Instance.SelectedFile.gameObject.AddComponent<ClickyTrigger>();
                ct.DialogueSequence = new[] { "ClickyEnding0" + UnityEngine.Random.Range(1, 4), "ClickyGoodbye" };
			}
            else if (State == 3)
            {
                Application.LoadLevel("RealLevel");
            }
		}
	}
}
