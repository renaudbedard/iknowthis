using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class GroundLevel : MonoBehaviour
{
	void Update()
	{
		var p = transform.position;
		p.y = 0.05f / 2 + 0.002f;
		transform.position = p;
	}
}
