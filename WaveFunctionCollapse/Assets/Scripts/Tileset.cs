using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tileset : MonoBehaviour
{
    public TextAsset tileData;
    public List<Texture2D> tiles;
    public List<string> tilenames;

    public Dictionary<string, Texture2D> tileLookup;

    public void Start()
    {
        tileLookup = new Dictionary<string, Texture2D>();
        for (int idx = 0; idx < tiles.Count; idx++)
        {
            tileLookup.Add(tilenames[idx], tiles[idx]);
        }
    }

    public (int[], int, int) getTilePixels(string tilename)
    {
        // Bitmaps are an array of integers, with each integer storing the pixel data in the format: 0xAARRGGBB
        Texture2D image;
        if (!tileLookup.TryGetValue(tilename, out image))
        {
            // If lookup fails, throw an error
            KeyNotFoundException exception = new KeyNotFoundException($"Tilename {tilename} not found in tileset {name}");
            throw exception;
        }

        int width = image.width, height = image.height;
        int[] result = new int[width * height];
        Color32[] pixelData = image.GetPixels32();
        for (int idx = 0; idx < height; idx++)
        {
            for (int jdx = 0; jdx < width; jdx++)
            {
                // Texture2D flattens the image data from leftt to right, bottom to top
                // To use this we have to invert that "bottom to top" situation
                int sourceIndex = idx * width + jdx;
                int resultIndex = (height - idx - 1) * width + jdx;
                result[resultIndex] = unchecked((int)0xff000000 | ((int)pixelData[sourceIndex].r << 16) | ((int)pixelData[sourceIndex].g << 8) | (int)pixelData[sourceIndex].b);
            }
        }
        return (result, width, height);
    }

    public static Texture2D pixelsToImage(int[] data, int width, int height)
    {
        Color32[] pixelData = new Color32[data.Length];
        for (int idx = data.Length - 1; idx >= 0; idx--)
        {
            pixelData[idx].b = (byte)(data[idx]);
            pixelData[idx].g = (byte)(data[idx] >> 8);
            pixelData[idx].r = (byte)(data[idx] >> 16);
            pixelData[idx].a = (byte)(data[idx] >> 24);
        }
        Texture2D image = new Texture2D(width, height);
        image.SetPixels32(pixelData);
        image.Apply();
        return image;
    }
}
