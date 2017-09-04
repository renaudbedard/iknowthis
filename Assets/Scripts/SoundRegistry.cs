using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;

class SoundRegistry : MonoBehaviour
{
	public static SoundRegistry Instance;

	void Start()
	{
		Instance = this;
	}

	public AudioClip[] ClickyTalk;
	public AudioClip[] Typing;
	public AudioClip TypeBell;
	public AudioClip DialoguePop;
	public AudioClip DialogueLeave;
	public AudioClip WinJingle;
    public AudioClip LoseJingle;
	public AudioClip MoveClick;
	public AudioClip ShortcutGo;
	public AudioClip ShortcutBack;
    public AudioClip SpotlightSpot;
    public AudioClip HideStart;
    public AudioClip HideEnd;
    public AudioClip WindowOpen;
    public AudioClip WrongLetter;
    public AudioClip Bump;
    public AudioClip FailHack;

    public AudioMixerSnapshot Normal;
    public AudioMixerSnapshot Hack;

    public AudioSource NormalSource;
    public AudioSource HackSource;

    public void FadeOut()
    {
        StartCoroutine(FadeOutCo());
    }

    IEnumerator FadeOutCo()
    {
        var audioSources = GetComponents<AudioSource>();

        while (audioSources[0].volume > 0)
        {
            foreach (var a in audioSources)
                a.volume -= 0.005f;
            yield return new WaitForEndOfFrame();
        }

        foreach (var a in audioSources)
            a.volume = 0;
    } 
}
