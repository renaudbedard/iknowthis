using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[SelectionBase]
class File : MonoBehaviour
{
	public enum FileTypes
	{
		Regular = 1,
		Corrupted,
		Hackable,
		Shortcut,
		Hidden,
		Honeypot
	}

	FileTypes oldType;
	public FileTypes Type;

	public bool IsTreasure;
	[HideInInspector]
	public float TreasureConfidence;
    [HideInInspector]
    public float ShownConfidence;
    [HideInInspector]
    public float SmoothConfidence;
	[HideInInspector]
	public bool Unlocked;
	[HideInInspector]
	public bool Disabled;

	[Serializable]
	public struct MaterialByType
	{
		public FileTypes Type;
		public Material Material;
	}
	public MaterialByType[] Materials;
	public Material LineMaterial;

	public GameObject HoneyPanelPrefab;
    Text honeyText;

	[HideInInspector]
	public Folder Parent;
	[HideInInspector]
	public int X;
	[HideInInspector]
	public int Y;
	[HideInInspector]
	public bool Selected;
	[HideInInspector]
	public bool IsBacklink;
	[HideInInspector]
	public bool Sunken;

	GameObject MeshObject;

	String Filename;
	bool lastHackView = true;

	GameObject Icon;

	void Start()
	{
		MeshObject = transform.Find("Mesh").gameObject;
		Refresh();
		Filename = FileSniffer.Instance.RandomFilename;

		transform.Find("Filename Text").GetComponent<TextMesh>().text = Filename;

		var iconTrans = transform.Find("Icon");
		if (iconTrans == null && Level.Instance != null && Level.Instance.IconPrefab != null)
		{
			var go = Instantiate(Level.Instance.IconPrefab) as GameObject;
			go.name = "Icon";
			go.transform.parent = transform;
			go.transform.localPosition = new Vector3(0, 0.51f, 0);
			go.transform.localScale = new Vector3(0.7381566f, 0.7381566f, 0.7381566f);
			Icon = go;
		}
		else if (iconTrans != null)
			Icon = iconTrans.gameObject;

        if (Type == FileTypes.Honeypot)
            honeyText = transform.Find("HoneyPanel").GetComponentInChildren<Text>();

		UpdateIcon();
	}

	public void UpdateIcon()
	{
		if (Icon == null || Type == 0)
			return;

	    var typeIcons = IconRegistry.Instance.IconTextures.First(x => x.Type == Type);
        var textures = typeIcons.Textures;

		Texture newTexture = null;
		if (Type == FileTypes.Hackable)
		{
			if (Disabled)
				newTexture = textures[2];
			else if (Unlocked)
				newTexture = textures[0];
			else
				newTexture = textures[1];
		}
		else if (Type == FileTypes.Honeypot)
		{
			// ?
			newTexture = textures[0];
		}
		else if (Type == FileTypes.Shortcut)
		{
			if (IsBacklink)
				newTexture = textures[1];
			else
				newTexture = textures[0];
		}
		else
		{
			newTexture = textures[UnityEngine.Random.Range(0, textures.Length)];
		}

		Icon.GetComponent<Renderer>().material.SetTexture("_MainTex", newTexture);
	}

	void Update()
	{
#if UNITY_EDITOR
		if (oldType != Type || MeshObject == null)
			Refresh();
#endif

		var p = transform.position;
		p.y = Selected ? 0.25f : transform.localScale.y / 2.0f + Parent.Floor.transform.localScale.y / 2.0f;
		if (Sunken)
			p.y = Parent.Floor.transform.localScale.y / 2.0f - transform.localScale.y / 2.0f + 0.001f;

		transform.position = Vector3.Lerp(transform.position, p, Sunken ? 0.1f : Selected ? 0.2f : 0.1f);

		if (PlayerControl.Instance != null &&
			lastHackView != PlayerControl.Instance.HackView && Type == FileTypes.Honeypot)
		{
			lastHackView = PlayerControl.Instance.HackView;
			var panel = transform.Find("HoneyPanel");
			panel.gameObject.SetActive(lastHackView);
		}

	    if (PlayerControl.Instance != null && Type == FileTypes.Honeypot && PlayerControl.Instance.HackView && honeyText != null)
	    {
            if (PlayerControl.Instance.SinceInHackView > 0.25f) 
                SmoothConfidence = Mathf.Lerp(SmoothConfidence, ShownConfidence, 0.025f);
            honeyText.text = (int)(SmoothConfidence * 100) + "%";
	    }
	}

	public void Refresh()
	{
		MeshObject = transform.Find("Mesh").gameObject;
		MeshObject.GetComponent<Renderer>().material = Materials.First(x => x.Type == Type).Material;

		if (Type == FileTypes.Shortcut)
		{
			if (GetComponent<Link>() == null)
				gameObject.AddComponent<Link>();
			GetComponent<Link>().From = this;
		}
		else if (Type != FileTypes.Shortcut && GetComponent<Link>() != null)
			DestroyImmediate(GetComponent<Link>());

		if (Type == FileTypes.Honeypot && transform.Find("HoneyPanel") == null)
		{
			var go = Instantiate(HoneyPanelPrefab) as GameObject;
			go.transform.parent = transform;
			go.transform.localPosition = Vector3.zero;
			go.name = "HoneyPanel";
		}
		else if (Type != FileTypes.Honeypot && transform.Find("HoneyPanel") != null)
			DestroyImmediate(transform.Find("HoneyPanel").gameObject);
	}

	void OnDrawGizmos()
	{
		if (Type == FileTypes.Shortcut && GetComponent<Link>().To != null)
			Gizmos.DrawLine(transform.position, GetComponent<Link>().To.transform.position);
	}
}
