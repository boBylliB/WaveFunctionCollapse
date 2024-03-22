using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Dropdown dropdown;
    public Texture2D outputImage;
    public Image outputImageHolder;

    private string chosenName = null;
    private List<string> names;

    private void Start()
    {
        updateDropdown();
    }

    public void setName(int index)
    {
        if (index < names.Count)
        {
            int stopIndex = names[index].IndexOf(':');
            chosenName = stopIndex > 0 ? names[index].Substring(0,names[index].IndexOf(':')) : names[index];
            UnityEngine.Debug.Log("Chose name " + chosenName);
        }
        else
            UnityEngine.Debug.Log($"Name not found for index {index}!");
    }

    public void startCollapse()
    {
        Stopwatch sw = Stopwatch.StartNew();
        var folder = System.IO.Directory.CreateDirectory("WFC/output");
        foreach (var file in folder.GetFiles()) file.Delete();

        System.Random random = new System.Random();
        XDocument xdoc = XDocument.Load("Assets/samples.xml");

        foreach (XElement xelem in xdoc.Root.Elements("simpletiled"))
        {
            WFCModel model;
            string name = xelem.Get<string>("name");
            if (name != chosenName)
                continue;
            UnityEngine.Debug.Log($"< {name}");

            int size = xelem.Get("size", 24);
            int width = xelem.Get("width", size);
            int height = xelem.Get("height", size);
            bool periodic = xelem.Get("periodic", false);
            string heuristicString = xelem.Get<string>("heuristic");
            var heuristic = heuristicString == "Scanline" ? WFCModel.Heuristic.Scanline : (heuristicString == "MRV" ? WFCModel.Heuristic.MRV : WFCModel.Heuristic.Entropy);

            string subset = xelem.Get<string>("subset");
            bool blackBackground = xelem.Get("blackBackground", false);
            model = new WaveFunctionCollapse(name, subset, width, height, periodic, blackBackground, heuristic);

            for (int i = 0; i < xelem.Get("screenshots", 2); i++)
            {
                for (int k = 0; k < 10; k++)
                {
                    Console.Write("> ");
                    int seed = random.Next();
                    bool success = model.Run(seed, xelem.Get("limit", -1));
                    if (success)
                    {
                        UnityEngine.Debug.Log("DONE");
                        model.Save($"WFC/output/{name} {seed}.png");
                        outputImage.LoadImage(System.IO.File.ReadAllBytes($"WFC/output/{name} {seed}.png"));
                        if (model is WaveFunctionCollapse stmodel && xelem.Get("textOutput", false))
                            System.IO.File.WriteAllText($"WFC/output/{name} {seed}.txt", stmodel.TextOutput());
                        break;
                    }
                    else UnityEngine.Debug.Log("CONTRADICTION");
                }
            }
        }

        UnityEngine.Debug.Log($"time = {sw.ElapsedMilliseconds}");
    }

    private void readNameOptions()
    {
        names = new List<string>();
        XDocument xdoc = XDocument.Load("Assets/samples.xml");

        foreach (XElement xelem in xdoc.Root.Elements("simpletiled"))
        {
            string current = xelem.Get<string>("name");
            if (names.Contains(current))
            {
                int size = xelem.Get("size", 24);
                string subset = xelem.Get<string>("subset");
                current += ": " + (subset != null ? subset + " " : "") + xelem.Get("width", size) + "x" + xelem.Get("height", size);
            }
            if (names.Contains(current))
            {
                string heuristic = xelem.Get<string>("heuristic");
                current += ", " + (xelem.Get("periodic", false) ? "periodic, " : "") + (heuristic != null ? heuristic + ", " : "") + (xelem.Get("blackBackground", false) ? "black background" : "");
            }
            names.Add(current);
        }
    }
    private void updateDropdown()
    {
        dropdown.ClearOptions();
        readNameOptions();
        dropdown.AddOptions(names);
        // Unity's drop down displays the first option initially, so we should select that as the first chosen name
        setName(0);
    }
}
