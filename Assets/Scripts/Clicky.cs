using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

class Clicky : MonoBehaviour
{
	public static Clicky Instance;

	Dictionary<string, string> Dialogue = new Dictionary<string, string>();

	public GameObject AssociatedWindow;
	Vector3 BaseScale;

	public bool Visible;
	ScaleTransition st;

	public event Action Dismissed;

	float animTimer;
	Image ClickyAvatar;
    Image ReturnIcon;
	bool clickyState;
    bool clickyState2;
    float iconTimer;
    float iconTimer2;

	public Sprite[] ClickySprites;

	void Awake()
	{
		var text = Resources.Load<TextAsset>("ClickyText");
		var lines = text.text.Split('|').Select(x => x.Trim());
		foreach (var l in lines)
		{
			var id = l.Substring(0, l.IndexOf('\n'));
			var talkie = l.Substring(l.IndexOf('\n') + 1);

			Dialogue.Add(id.Trim(), talkie.Trim());
			//Debug.Log("id = " + id + " | talkie = " + talkie);
		}

		Instance = this;

		st = gameObject.AddComponent<ScaleTransition>();

		BaseScale = AssociatedWindow.transform.localScale;
		AssociatedWindow.transform.localScale = Vector3.zero;

		ClickyAvatar = AssociatedWindow.transform.Find("Image").GetComponent<Image>();
	    ReturnIcon = AssociatedWindow.transform.Find("Return").GetComponent<Image>();
	}

	public void Popup(string text, float length = 0.25f)
	{
		//Debug.Log("Popup : " + text);
        st.TransitionOver = length;
		st.Begin(Vector3.zero, BaseScale, AssociatedWindow.transform, () =>
		{
			//Debug.Log("Visible!");
			Visible = true;
			GetComponent<AudioSource>().PlayOneShot(SoundRegistry.Instance.ClickyTalk[UnityEngine.Random.Range(0, SoundRegistry.Instance.ClickyTalk.Length)]);
		});

	    var textNode = AssociatedWindow.GetComponentInChildren<Text>();
		textNode.text = Dialogue[text];

	    var rt = AssociatedWindow.GetComponent<RectTransform>();
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 700 + textNode.preferredHeight * 1.911035f);

	    iconTimer = 0;
        ReturnIcon.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
	}

	public void Dismiss(float length = 0.125f)
	{
	    st.TransitionOver = length;
		st.Begin(AssociatedWindow.transform.localScale, Vector3.zero, AssociatedWindow.transform, () =>
		{
			//Debug.Log("Hidden!");
			Visible = false;
			if (Dismissed != null)
			{
				var d = Dismissed;
				Dismissed = null;
				d();
			}
		});
	}

	void Update()
	{
        animTimer += Time.deltaTime;

		if (animTimer > 1)
		{
			animTimer -= 1f;
            clickyState = !clickyState;
		}

	    if (clickyState)
	    {
            iconTimer2 += Time.deltaTime;
            if (iconTimer2 > 0.15)
            {
                iconTimer2 -= 0.15f;
                clickyState2 = !clickyState2;
            }
	    }

	    iconTimer += Time.deltaTime;
        ReturnIcon.color = new Color(1.0f, 1.0f, 1.0f, Easing.EaseIn(Mathf.Clamp01((iconTimer - 2.0f) * 5.0f), EasingType.Quintic));

        ClickyAvatar.sprite = clickyState2 ? ClickySprites[0] : ClickySprites[1];
	}
}
