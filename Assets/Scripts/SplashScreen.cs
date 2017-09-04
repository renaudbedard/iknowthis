using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

class SplashScreen : MonoBehaviour
{
    public static SplashScreen Instance { get; private set; }

    void Start()
    {
        Instance = this;
    }

    public void Dismiss()
    {
        var st = GetComponentInChildren<ScaleTransition>();
        st.Begin(Vector3.one, Vector3.zero, st.transform, () => Destroy(gameObject));
        transform.Find("Fade").GetComponent<Image>().CrossFadeAlpha(0.0f, 0.25f, false);
    }
}
