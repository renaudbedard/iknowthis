using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class CameraController : MonoBehaviour
{
	const float ShakeDuration = 0.5f;
	const float ShakeStrength = 0.625f;

    float shakeCustomStrength, shakeCustomLength;

	public static CameraController Instance
	{
		get;
		private set;
	}

	Vector3 smoothPosition;
	Vector3 smoothLookAt;

	Vector3 honeyCenter;

	bool doShake;
	float sinceShakeStarted;

    bool percentagesShown;
    bool firstExitFromHackView;

    public GameObject FailCam;

	public void Shake(float strength = 1)
	{
		doShake = true;
	    shakeCustomStrength = strength;
	    shakeCustomLength = strength;
		sinceShakeStarted = 0;
	}

	void Awake()
	{
		Instance = this;
	    FailCam = transform.Find("FailCam").gameObject;
	}
	
	void Start()
	{
		var honeypots = GameObject.Find("Level Root")
		          .GetComponentsInChildren<File>()
		          .Where(x => x.Type == File.FileTypes.Honeypot).ToArray();

		honeyCenter = honeypots.Aggregate(Vector3.zero, (t, n) => t + n.transform.position) / honeypots.Length;
	}

	void Update()
	{
		var player = PlayerControl.Instance;
		var folder = player.CurrentFolder;
		var camera = GetComponent<Camera>();

		var midPoint = Vector3.Lerp(player.SelectedFile.transform.position, folder.transform.position, 0.25f);

		var destPosition = new Vector3(folder.transform.position.x, folder.transform.position.y, midPoint.z)
			+ new Vector3(0, 1.5f, -folder.Height / 2.0f - 1);

		var interSpeed = 0.1f;

	    if (player.HackView)
	    {
	        if (!percentagesShown)
            {
	            var ct = PlayerControl.Instance.SelectedFile.gameObject.AddComponent<ClickyTrigger>();
                ct.DialogueSequence = new[] { "ClickyGuideHack01", "ClickyGuideHack05" };
	            ct.SayOnce = true;
	            percentagesShown = true;
	        }

	        destPosition = honeyCenter + new Vector3(0, 15, 0);
			interSpeed = 0.05f;
		}
        else if (percentagesShown && !firstExitFromHackView)
        {
            var ct = PlayerControl.Instance.SelectedFile.gameObject.AddComponent<ClickyTrigger>();
            ct.DialogueSequence = new[] { "ClickyGuideHack06", "ClickyGuideAdminScan02" };
            ct.SayOnce = true;
            firstExitFromHackView = true;
        }

		smoothPosition = Vector3.Lerp(smoothPosition, destPosition, interSpeed);

		if (doShake)
		{
			sinceShakeStarted += Time.deltaTime;

            smoothPosition += UnityEngine.Random.onUnitSphere * ShakeStrength * shakeCustomStrength * (ShakeDuration * shakeCustomLength - sinceShakeStarted);
		}

		camera.transform.position = smoothPosition;

		var destinationLookAt = (player.SelectedFile.transform.position + folder.transform.position) / 2.0f;
		var worldUp = Vector3.up;

		if (player.HackView)
		{
			destinationLookAt = honeyCenter;
			worldUp = Vector3.forward;
		}

		smoothLookAt = Vector3.Lerp(smoothLookAt, destinationLookAt, interSpeed);

		if (doShake)
		{
            smoothLookAt += UnityEngine.Random.onUnitSphere * ShakeStrength * shakeCustomStrength * (ShakeDuration * shakeCustomLength - sinceShakeStarted);

            if (sinceShakeStarted > ShakeDuration * shakeCustomLength)
			{
				sinceShakeStarted = 0;
				doShake = false;
			}
		}

		camera.transform.LookAt(smoothLookAt, worldUp);
	}
}
