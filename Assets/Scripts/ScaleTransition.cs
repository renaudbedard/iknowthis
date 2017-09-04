using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class ScaleTransition : MonoBehaviour
{
	Vector3 fromScale;
	Vector3 scaleDestination;
	float sinceStarted;
	public float TransitionOver = 0.25f;
	Transform forTransform;
	Action OnComplete;
	bool active;
    bool inverse;

	public void Begin(Vector3 from, Vector3 to, Transform forTransform, Action onComplete)
	{
		fromScale = from;
		scaleDestination = to;
		sinceStarted = 0;
		this.forTransform = forTransform;
		OnComplete = onComplete;
		active = true;
	    inverse = to.x < from.x;

        //if (!inverse)
	    {
            if (forTransform.GetComponent<AudioSource>())
	            forTransform.GetComponent<AudioSource>().PlayOneShot(SoundRegistry.Instance.WindowOpen);
	    }
	}

	void Update()
	{
		if (!active)
			return;

		sinceStarted += Time.deltaTime;

	    forTransform.localScale = Vector3.Lerp(fromScale, scaleDestination,
	        inverse
	            ? Easing.EaseOut(Mathf.Clamp01(sinceStarted / TransitionOver), EasingType.Quartic)
                : Easing.EaseIn(Mathf.Clamp01(sinceStarted / TransitionOver), EasingType.Quartic));

		if (sinceStarted >= TransitionOver)
		{
			active = false;
			if (OnComplete != null)
			{
				var oc = OnComplete;
				OnComplete = null;
				oc();
			}
		}
	}
}
