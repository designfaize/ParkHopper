using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class TileBehavior : MonoBehaviour {

	public enum TileType
	{
		Pay,
		FastPass,
		Get,
		ForceMove,
		Lose,
		Empty
	}
	public TileType tile;
	public int value;
	public TextMeshPro textMesh;
	void Start()
	{
		if (textMesh != null)
			textMesh.SetText(value.ToString ());
	}
}
