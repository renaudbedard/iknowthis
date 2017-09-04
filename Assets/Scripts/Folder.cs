using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[ExecuteInEditMode]
[SelectionBase]
class Folder : MonoBehaviour
{
	int oldWidth;
	public int Width;
	int oldHeight;
	public int Height;

	File[] Files;
	GameObject FileRoot;

	public GameObject Floor;

	public File FileTemplate;

	void Start()
	{
		Files = new File[0];
		Refresh();
	}
	void Awake()
	{
		Refresh();
	}

	void Update()
	{
#if UNITY_EDITOR
		if (oldHeight != Height || oldWidth != Width || Floor == null || FileRoot == null)
			Refresh();
#endif
	}

	void Refresh()
	{
		Width = Math.Max(Width, 0);
		Height = Math.Max(Height, 0);

		FileRoot = transform.Find("Files").gameObject;
		Files = FileRoot.GetComponentsInChildren<File>().ToArray();

		for (int i = Math.Min(Width * Height, Files.Length); i < Files.Length; i++)
		{
			if (Files[i] != null)
				DestroyImmediate(Files[i].gameObject);
		}

		Array.Resize(ref Files, Width * Height);
		for (int i = 0; i < Width * Height; i++)
		{
			if (Files[i] == null)
			{
				Files[i] = Instantiate(FileTemplate) as File;
			}

			Files[i].transform.parent = FileRoot.transform;

			int x, y;
			GetCoord(i, out x, out y);

			Files[i].transform.localPosition = new Vector3(x - Width / 2.0f + 0.5f, 0.125f, y - Height / 2.0f + 0.5f);
			Files[i].transform.localScale = new Vector3(0.5f, 0.125f, 0.5f);
			Files[i].X = x; Files[i].Y = y;
			Files[i].Parent = this;
			Files[i].name = "File (" + x + ", " + y + ")";
		}

		Floor = transform.Find("Floor").gameObject;
		Floor.transform.localScale = new Vector3(Width, 0.05f, Height);

		oldWidth = Width;
		oldHeight = Height;
	}

	public File FileAt(int i, int j)
	{
		return Files[j * Width + i];
	}
	public void GetCoord(int index, out int i, out int j)
	{
		i = index % Width;
		j = index / Width;
	}
}
