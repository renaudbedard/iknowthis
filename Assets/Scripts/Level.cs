using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Level : MonoBehaviour
{
	File[] Honeypots;
	int hackspotCount;

	float confidenceLevel;
    public bool SeenFirstSentinel;

    int hacks;

	public static Level Instance
	{
		get;
		private set;
	}

	void Awake()
	{
		Instance = this;
	    SeenFirstSentinel = false;

        Cursor.visible = false;
	}

    public void Start()
    {
        Honeypots = GetComponentsInChildren<File>().Where(x => x.Type == File.FileTypes.Honeypot).ToArray();
        hackspotCount = GetComponentsInChildren<File>().Count(x => x.Type == File.FileTypes.Hackable);

        // randomize treasure
        foreach (var h in Honeypots)
            h.IsTreasure = false;
        if (Honeypots.Length > 0)
            Honeypots[UnityEngine.Random.Range(0, Honeypots.Length)].IsTreasure = true;

        foreach (var h in Honeypots)
            h.TreasureConfidence = 1;
    }

	public void IncreaseConfidence()
	{
	    hacks++;

        var activePots = Honeypots.Where(x => x.TreasureConfidence == 1 && !x.IsTreasure).ToArray();
	    var potCount = activePots.Length;
	    if (hacks > 1 && potCount > 0)
	    {
	        activePots[UnityEngine.Random.Range(0, activePots.Length)].TreasureConfidence = 0;
	        potCount--;
	    }

	    foreach (var h in Honeypots)
            h.ShownConfidence = Mathf.Clamp01(h.TreasureConfidence * (1.0f / (potCount + 1) + UnityEngine.Random.Range(-0.02f, 0.02f)));
	}

	public Folder RootFolder;
	public File StartFile;
	public GameObject IconPrefab;
}