using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

class HackerTyper : MonoBehaviour
{
    const int LinesRequired = 10;
    const float BaseAdminScanTime = 35;

    float curAdminScanTime = BaseAdminScanTime;

	string[] codeLines;
	string revealedCode;
	int revealedLines;
	int revealedCharsOnCurLine;
	int excessChars;
	bool inError;
    bool hiding;

    float sinceStartedHacking;
	float sinceStartHolding;
	float sinceBackspaced;
	float blinky;
	bool blinkyState;

	public bool Visible;
    GameObject adminWindow;
    ScaleTransition adminSt;
    GameObject adminMeter;

	File hackedFile;

	ScaleTransition st;

    bool shownOnce;

	void Awake()
	{
		var t = Resources.Load<TextAsset>("kernel");
		codeLines = t.text.Split('\n').Select(x => x.TrimEnd()).Where(x => x.Length > 0).ToArray();

		st = gameObject.AddComponent<ScaleTransition>();

	    adminWindow = transform.parent.Find("Scan Window").gameObject;
	    adminMeter = adminWindow.transform.Find("Scan Meter").gameObject;

        adminSt = adminWindow.AddComponent<ScaleTransition>();

	    adminWindow.transform.localScale = Vector3.zero;
		transform.parent.localScale = Vector3.zero;
		Visible = false;
	}

	public void ShowFor(File file)
	{
		GetComponent<UnityEngine.UI.Text>().text = "";
		revealedLines = UnityEngine.Random.Range(0, codeLines.Length);
		hackedFile = file;

	    sinceStartedHacking = 0;

		st.Begin(Vector3.zero, Vector3.one, transform.parent, () => Visible = true);

	    if (!shownOnce)
	    {
	        var ct = PlayerControl.Instance.SelectedFile.gameObject.AddComponent<ClickyTrigger>();
            ct.DialogueSequence = new[] { "ClickyGuideHack04", "ClickyGuideHack07" };
	        ct.SayOnce = true;
	        shownOnce = true;
            StartCoroutine(WaitThenTogglePlay(SoundRegistry.Instance.HackSource, 0.25f));
	    }
	    else
            SoundRegistry.Instance.HackSource.Play();

        StartCoroutine(WaitThenTogglePlay(SoundRegistry.Instance.NormalSource, 0.25f));

        SoundRegistry.Instance.Hack.TransitionTo(0.25f);
        
	    StartCoroutine(DelayShow());

	}

    IEnumerator DelayShow()
    {
        yield return new WaitForSeconds(0.25f);
        adminSt.Begin(Vector3.zero, Vector3.one / 2, adminWindow.transform, () => { });
    } 

	void Backspace()
	{
		if (excessChars > 0)
		{
			excessChars--;
			revealedCode = revealedCode.Substring(0, revealedCode.Length - 1);
			if (excessChars == 0)
			{
				revealedCode = revealedCode.Substring(0, revealedCode.Length - "<color=#ff0000ff>".Length);
				inError = false;
			}
		}
		else if (revealedCharsOnCurLine > 0)
		{
			revealedCharsOnCurLine--;
			revealedCode = revealedCode.Substring(0, revealedCode.Length - 1);
		}
	}

	void Hide()
	{
	    hiding = true;
        st.Begin(Vector3.one, Vector3.zero, transform.parent, () => { Visible = false; hiding = false; adminWindow.transform.localScale = Vector3.zero; });

        SoundRegistry.Instance.Normal.TransitionTo(0.5f);

        SoundRegistry.Instance.NormalSource.Play();
	    StartCoroutine(WaitThenTogglePlay(SoundRegistry.Instance.HackSource, 0.5f));
	}

    IEnumerator WaitThenTogglePlay(AudioSource s, float time)
    {
        yield return new WaitForSeconds(time);
        if (!s.isPlaying)   s.Play();
        else                s.Pause();
    } 

    void Fail() 
    {
        revealedCharsOnCurLine = 0;
        revealedLines = 0;
        hackedFile.Disabled = true;
        hackedFile.UpdateIcon();
        inError = false;
        revealedCode = "";

        adminWindow.GetComponentInChildren<AudioSource>().PlayOneShot(SoundRegistry.Instance.FailHack);

        Hide();
    }

    float frac(float x)
    {
        return x - (int) x;
    }

	void Update()
	{
        if (!Visible || hackedFile.Disabled || hiding)
			return;

	    if (!Clicky.Instance.Visible)
	        sinceStartedHacking += Time.deltaTime;

	    float timePortion = Mathf.Clamp01(sinceStartedHacking / curAdminScanTime);

	    var orange = new Color(212 / 255.0f, 112 / 255.0f, 40 / 255.0f, 1.0f);

        adminMeter.transform.localScale = new Vector3(timePortion, 1, 1);
	    adminMeter.GetComponentInChildren<Image>().color = timePortion > 0.875
            ? (frac(sinceStartedHacking * 2.0f) < 0.5 ? new Color(1, 0, 0, 0) : Color.Lerp(orange, Color.red, 0.75f * (timePortion - 0.875f) / (1 - 0.875f))) : orange;

        if (sinceStartedHacking > curAdminScanTime)
        {
            curAdminScanTime += 5;
            Fail();
            return;
	    }

		bool textDirty = false;
		var curLine = codeLines[revealedLines];

        if (!Clicky.Instance.Visible)
		for (KeyCode l = 0; (int)l < 319; l++)
			if (Input.GetKeyDown(l))
			{
                //Debug.Log("Key press : " + l);

				textDirty = true;

				if (l == KeyCode.Escape)
				{
				    Fail();
				    return;
				}

				if (l == KeyCode.Return || l == KeyCode.KeypadEnter)
				{
                    //Debug.Log("Chars match : " + (revealedCharsOnCurLine == curLine.Length) + " | excess : " + excessChars);

					if (revealedCharsOnCurLine == curLine.Length && excessChars == 0)
					{
						revealedLines++;
						if (revealedLines == codeLines.Length)
							revealedLines = 0;
						revealedCharsOnCurLine = 0;
						inError = false;
						revealedCode += "\n";
						transform.parent.GetComponent<AudioSource>().PlayOneShot(SoundRegistry.Instance.TypeBell);
						if (revealedCode.Count(x => x == '\n') >= LinesRequired)
						{
						    curAdminScanTime -= 3;
							revealedCode = "";
							Level.Instance.IncreaseConfidence();
							hackedFile.Unlocked = true;
							hackedFile.UpdateIcon();
							PlayerControl.Instance.HackView = true;
							Hide();
							return;
						}
					}
					else
					{
                        GetComponent<AudioSource>().pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                        GetComponent<AudioSource>().PlayOneShot(SoundRegistry.Instance.WrongLetter);
					}
					continue;
				}

				if (l == KeyCode.Backspace || l == KeyCode.Delete)
				{
					sinceStartHolding = 0;
					Backspace();
                    GetComponent<AudioSource>().pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                    GetComponent<AudioSource>().PlayOneShot(SoundRegistry.Instance.Typing[UnityEngine.Random.Range(0, SoundRegistry.Instance.Typing.Length)]);
					continue;
				}

				int newlyRevealed = UnityEngine.Random.Range(1, 6);
				if (curLine.Length <= revealedCharsOnCurLine + newlyRevealed)
				{
					if (revealedCharsOnCurLine < curLine.Length)
					{
                        GetComponent<AudioSource>().pitch = UnityEngine.Random.Range(0.8f, 1.2f);
						GetComponent<AudioSource>().PlayOneShot(SoundRegistry.Instance.Typing[UnityEngine.Random.Range(0, SoundRegistry.Instance.Typing.Length)]);

						revealedCode += curLine.Substring(revealedCharsOnCurLine, curLine.Length - revealedCharsOnCurLine);
						revealedCharsOnCurLine = curLine.Length;
					}
					else
					{
					    GetComponent<AudioSource>().pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                        GetComponent<AudioSource>().PlayOneShot(SoundRegistry.Instance.WrongLetter);

                        if (!inError)
						{
							revealedCode += "<color=#ff0000ff>";
							inError = true;
						}
						revealedCode += (char) UnityEngine.Random.Range(32, 127);
						excessChars++;
					}
				}
				else
				{
					revealedCode += curLine.Substring(revealedCharsOnCurLine, newlyRevealed);
					revealedCharsOnCurLine += newlyRevealed;
                    GetComponent<AudioSource>().pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                    GetComponent<AudioSource>().PlayOneShot(SoundRegistry.Instance.Typing[UnityEngine.Random.Range(0, SoundRegistry.Instance.Typing.Length)]);
				}
			}

		if (!textDirty)
		{
			// backspace hold thing
			if (Input.GetKey(KeyCode.Backspace))
			{
				sinceStartHolding += Time.deltaTime;

				if (sinceStartHolding > 0.15)
				{
					sinceBackspaced += Time.deltaTime;
					if (sinceBackspaced > 0.025)
					{
						sinceBackspaced -= 0.025f;
						Backspace();
						textDirty = true;
					}
				}
			}
			else
				sinceStartHolding = 0;
		}

		blinky += Time.deltaTime;
		if (blinky > 0.375)
		{
			blinky -= 0.375f;
			blinkyState = !blinkyState;
			textDirty = true;
		}

		if (textDirty)
		{
			GetComponent<UnityEngine.UI.Text>().text = revealedCode;
			if (inError)
				GetComponent<UnityEngine.UI.Text>().text += "</color>";
			if (blinkyState)
				GetComponent<UnityEngine.UI.Text>().text += "<b>_</b>";
		}
	}
}
