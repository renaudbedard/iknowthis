using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Vectrosity;

class Link : MonoBehaviour
{
	[HideInInspector]
	public File From;
	[HideInInspector]
	public Material LineMaterial;

	public File To;
    public Vector3 ForcedExtentDirection;
    public Vector3 ForcedInvExtentDirection;

	VectorLine inStraight, inter, outStraight;

	void Start()
	{
		VectorLine.SetCamera3D(Camera.main);

		if (!GetComponent<File>().IsBacklink)
		{
			var from = From.transform.position;
			var to = To.transform.position;
			from.y = -0.05f / 2;
			to.y = -0.05f / 2;

			Vector3 extentDirection = Vector3.zero;
			var diff = to - from;
			if (Math.Abs(diff.x) > Math.Abs(diff.z))
				extentDirection = Math.Sign(diff.x) * Vector3.right;
			else
				extentDirection = Math.Sign(diff.z) * Vector3.forward;

		    if (ForcedExtentDirection != Vector3.zero)
		        extentDirection = ForcedExtentDirection;

		    var invExtent = -extentDirection;
            if (ForcedInvExtentDirection != Vector3.zero)
                invExtent = ForcedInvExtentDirection;

			inStraight = new VectorLine("In", new[] { from, from + extentDirection }, LineMaterial, 1.5f);
            inter = new VectorLine("Inter", new[] { from + extentDirection, to + invExtent }, LineMaterial, 1.5f);
            outStraight = new VectorLine("Out", new[] { to + invExtent, to }, LineMaterial, 1.5f);
		}

		if (To.Type != File.FileTypes.Shortcut)
		{
			To.Type = File.FileTypes.Shortcut;
			To.IsBacklink = true;
			To.Refresh();
			To.UpdateIcon();
			To.GetComponent<Link>().To = GetComponent<File>();
		}
	}

	void OnDestroy()
	{
		if (inStraight != null)
			VectorLine.Destroy(ref inStraight);
		if (inter != null)
			VectorLine.Destroy(ref inter);
		if (outStraight != null)
			VectorLine.Destroy(ref outStraight);
	}

	void Update()
	{
	}

	void OnGUI()
	{
		if (!GetComponent<File>().IsBacklink && inStraight != null && inter != null && outStraight != null)
		{
			inStraight.Draw3D();
			inter.Draw3D();
			outStraight.Draw3D();
		}
	}
}
