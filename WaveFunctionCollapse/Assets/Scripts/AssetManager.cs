using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class AssetManager : MonoBehaviour
{
    public TextAsset tilesetData;
    public Dictionary<string, Tileset> tilesetLookup;

    public void Start()
    {
        tilesetLookup = new Dictionary<string, Tileset>();
        Tileset[] tilesets = GetComponentsInChildren<Tileset>();
        foreach (Tileset tileset in tilesets)
        {
            tilesetLookup.Add(tileset.name, tileset);
        }
    }
}