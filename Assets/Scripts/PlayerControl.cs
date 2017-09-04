using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

class PlayerControl : MonoBehaviour
{
	public static PlayerControl Instance { get; private set; }

	void Awake()
	{
		Instance = this;
	}

	public Folder CurrentFolder;
	public File SelectedFile;
	public GameObject HackingWindow;
	public bool HackView;
    public float SinceInHackView;
    bool firstCorrupt;
    bool firstRehack;
    bool firstShortcut;
    bool dead;
    public RawImage FailView;

    float toClickyPest;

	public int X, Y;

	void Start()
	{
		WarpToStart();

	    toClickyPest = UnityEngine.Random.Range(5, 25);
	}

	public void WarpToStart()
	{
		if (SelectedFile != null)
		{
			SelectedFile.Selected = false;
			SelectedFile.Sunken = false;
		}

		CurrentFolder = Level.Instance.RootFolder;
		SelectedFile = Level.Instance.StartFile;

		X = SelectedFile.X;
		Y = SelectedFile.Y;

		SelectedFile.Selected = true;
	}

    void HandleMovement()
    {
        bool refreshSelection = false;
        int oldX = X, oldY = Y;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            X = Math.Max(0, X - 1);
            refreshSelection = true;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            X = Math.Min(CurrentFolder.Width - 1, X + 1);
            refreshSelection = true;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Y = Math.Min(CurrentFolder.Height - 1, Y + 1);
            refreshSelection = true;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Y = Math.Max(0, Y - 1);
            refreshSelection = true;
        }

        if (refreshSelection)
        {
            var newSelection = CurrentFolder.FileAt(X, Y);
            if (newSelection.Type == File.FileTypes.Corrupted)
            {
                if (!firstCorrupt)
                {
                    var ct = SelectedFile.gameObject.AddComponent<ClickyTrigger>();
                    ct.DialogueSequence = new[] { "ClickyGuideCorrupted" };
	                ct.SayOnce = true;
                    firstCorrupt = true;
                }

                GetComponent<AudioSource>().PlayOneShot(SoundRegistry.Instance.Bump);

                CameraController.Instance.Shake(0.175f);

                X = oldX;
                Y = oldY;
            }
            else
            {
                if (newSelection != SelectedFile)
                {
                    SelectedFile.Selected = false;
                    SelectedFile.Sunken = false;
                    newSelection.Selected = true;

                    GetComponent<AudioSource>().PlayOneShot(SoundRegistry.Instance.MoveClick);

                    bool noRandom = false;

                    if (newSelection.Type == File.FileTypes.Hackable && newSelection.Unlocked && !firstRehack)
                    {
                        var ct = newSelection.gameObject.AddComponent<ClickyTrigger>();
                        ct.DialogueSequence = new[] { "ClickyGuideHack08" };
                        ct.SayOnce = true;
                        firstRehack = true;
                        noRandom = true;
                    }

                    if (newSelection.Type == File.FileTypes.Shortcut && !firstShortcut)
                    {
                        var ct = newSelection.gameObject.AddComponent<ClickyTrigger>();
                        ct.DialogueSequence = new[] { "ClickyGuideShortcut", "ClickySendOff" };
                        ct.SayOnce = true;
                        firstShortcut = true;
                        noRandom = true;
                    }
                }

                SelectedFile = newSelection;
            }
        }
    }

	void Update()
	{
	    if (SplashScreen.Instance)
	    {
	        if (Input.GetKeyDown(KeyCode.Return))
	            SplashScreen.Instance.Dismiss();
            else if (Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();
	        return;
	    }

		if (Clicky.Instance.Visible)
		{
		    if (Input.GetKeyDown(KeyCode.Return))
		    {
		        Clicky.Instance.Dismiss();
                if (dead)
                    Application.LoadLevel("RealLevel");
		    }

		    if (!WinFlow.Instance.started && !HackView && !HackingWindow.GetComponentInChildren<HackerTyper>().Visible)
                HandleMovement();
			return;
		}

	    if (WinFlow.Instance.started)
			return;

		if (HackingWindow.GetComponentInChildren<HackerTyper>().Visible)
			return;

		if (HackView)
		{
		    SinceInHackView += Time.deltaTime;

			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
			{
				HackView = false;
			}
			return;
		}
		SinceInHackView = 0;

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

	    if (SelectedFile.GetComponent<ClickyTrigger>() == null || SelectedFile.GetComponent<ClickyTrigger>().Done)
	    {
	        toClickyPest -= Time.deltaTime;
	        if (toClickyPest < 0)
	        {
	            Clicky.Instance.Popup("ClickyPest" + UnityEngine.Random.Range(1, 11));
	            toClickyPest = UnityEngine.Random.Range(5, 25);
	        }
	    }

	    HandleMovement();

		if (Input.GetKeyDown(KeyCode.Return))
		{
			switch (SelectedFile.Type)
			{
				case File.FileTypes.Shortcut:
					{
						var link = SelectedFile.GetComponent<Link>();

						if (SelectedFile.IsBacklink)
							GetComponent<AudioSource>().PlayOneShot(SoundRegistry.Instance.ShortcutBack);
						else
							GetComponent<AudioSource>().PlayOneShot(SoundRegistry.Instance.ShortcutGo);

						var newSelection = link.To;
						SelectedFile.Selected = false;
						newSelection.Selected = true;
						SelectedFile = newSelection;
						X = SelectedFile.X;
						Y = SelectedFile.Y;
						CurrentFolder = SelectedFile.Parent;
					}
					break;

				case File.FileTypes.Hackable:
					{
						if (!SelectedFile.Disabled)
						{
							if (SelectedFile.Unlocked)
							{
								HackView = true;
							}
							else
								HackingWindow.GetComponentInChildren<HackerTyper>().ShowFor(SelectedFile);
						}
					}
					break;

				case File.FileTypes.Honeypot:
					{
						if (SelectedFile.IsTreasure)
						{
							WinFlow.Instance.Win();
						}
						else
						{
							Clicky.Instance.Popup("ClickyPlayerDeath0" + UnityEngine.Random.Range(1, 4));
                            GetComponent<AudioSource>().PlayOneShot(SoundRegistry.Instance.LoseJingle);
						    dead = true;
                            SoundRegistry.Instance.FadeOut();
						    CameraController.Instance.FailCam.SetActive(true);
                            FailView.material.SetFloat("_StartTime", Time.timeSinceLevelLoad);
                            FailView.gameObject.SetActive(true);
						}
					}
					break;
			}
		}

	    if (SelectedFile.Type == File.FileTypes.Hidden)
	    {
            bool nowSunken = Input.GetKey(KeyCode.Space);

            if (nowSunken && !SelectedFile.Sunken)
                GetComponent<AudioSource>().PlayOneShot(SoundRegistry.Instance.HideStart);
            else if (!nowSunken && SelectedFile.Sunken)
                GetComponent<AudioSource>().PlayOneShot(SoundRegistry.Instance.HideEnd);

	        SelectedFile.Sunken = nowSunken;
	    }
	}
}
