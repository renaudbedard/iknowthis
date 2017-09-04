using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[SelectionBase]
class Sentinel : MonoBehaviour
{
	public struct PathPair
	{
		public File From;
		public File To;

		public float Distance()
		{
			return Vector3.Distance(From.transform.position, To.transform.position);
		}
	}

	public float Speed;

	public File[] Path;
	List<PathPair> PathPairs;

	public File AssociatedHackPoint;

	Renderer[] renderers;

	int currentPair;
	bool Visible;
	float sinceStartedPair;
	bool backwards;

	float sinceVisible;
    float leftToWait;

	Vector3 smoothPosition;
    float sinceHit;

	void Start()
	{
		PathPairs = new List<PathPair>();

		for (int i = 0; i < Path.Length - 1; i++)
			PathPairs.Add(new PathPair { From = Path[i], To = Path[i + 1] });

		renderers = GetComponentsInChildren<Renderer>();

		smoothPosition = Path[0].transform.position;
		smoothPosition.y = 0.05f / 2 + 0.001f;
		transform.position = smoothPosition;

		// debug
		//Visible = true;

	    leftToWait = 2.0f;
	}

	void Update()
	{
	    sinceHit += Time.deltaTime;

		if (!Visible)
		{
			Visible = AssociatedHackPoint.Disabled || AssociatedHackPoint.Unlocked;
		}

		if (Visible)
		{
		    if (!PlayerControl.Instance.HackView)
		    {
                if (!Level.Instance.SeenFirstSentinel)
                {
                    var ct = PlayerControl.Instance.SelectedFile.gameObject.AddComponent<ClickyTrigger>();
                    ct.DialogueSequence = new[] { "ClickyGuideAdminScan01", "ClickyGuideAdminScan02" };
                    ct.SayOnce = true;

                    Level.Instance.SeenFirstSentinel = true;
                }

		        if (leftToWait > 0)
		            leftToWait = Math.Max(0, leftToWait - Time.deltaTime * Speed);
		        else
		            sinceStartedPair += Time.deltaTime * Speed;
		    }

		    var p = PathPairs[currentPair];
			var step = Mathf.Clamp01(sinceStartedPair / p.Distance());

			var from = backwards ? p.To.transform.position : p.From.transform.position;
			var to = backwards ? p.From.transform.position : p.To.transform.position;

			var newPos = Vector3.Lerp(from, to, step);
			newPos.y = 0.05f / 2 + 0.001f;

			smoothPosition = newPos;

			if (step >= 1)
			{
				currentPair += backwards ? -1 : 1;
				if (currentPair == PathPairs.Count || currentPair == -1)
				{
					backwards = !backwards;
					currentPair = Mathf.Clamp(currentPair, 0, PathPairs.Count - 1);
				}
				sinceStartedPair = 0;
			    leftToWait = 1;
			}

			transform.position = Vector3.Lerp(transform.position, smoothPosition, 0.15f);

			// detect player
			if (sinceHit > 0.5 &&
                Vector3.Distance(PlayerControl.Instance.SelectedFile.transform.position, transform.position) < 0.625f &&
				!PlayerControl.Instance.SelectedFile.Sunken)
			{
                GetComponent<AudioSource>().PlayOneShot(SoundRegistry.Instance.SpotlightSpot);
				CameraController.Instance.Shake();
				PlayerControl.Instance.WarpToStart();
			    sinceHit = 0;
			}
		}

		if (Visible && sinceVisible < 1)
			sinceVisible += Time.deltaTime * 2;
		else if (!Visible && sinceVisible > 0)
			sinceVisible -= Time.deltaTime * 2;

		foreach (var r in renderers)
			r.material.SetVector("_Color", new Vector4(1, 1, 1, sinceVisible));
	}
}
