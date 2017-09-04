using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class IconRegistry : MonoBehaviour
{
	public static IconRegistry Instance;

	void Start()
	{
		Instance = this;
	}

	[Serializable]
	public struct TextureCollection
	{
		public File.FileTypes Type;
		public Texture[] Textures;
	}
	public TextureCollection[] IconTextures;
}
