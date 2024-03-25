using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Dropdown dropdown;
    public Image outputImage;

    public AssetManager am;

    public Slider widthSlider;
    public TextMeshProUGUI widthLabel;
    public Slider heightSlider;
    public TextMeshProUGUI heightLabel;
    public Slider attemptsSlider;
    public TextMeshProUGUI attemptsLabel;

    private string chosenName = null;
    private List<string> names;

    private int width = 2;
    private int height = 2;
    private int maxAttempts = 10;

    private void Start()
    {
        updateDropdown();
    }

    public void setName(int index)
    {
        if (index < names.Count)
        {
            int stopIndex = names[index].IndexOf(':');
            chosenName = stopIndex > 0 ? names[index].Substring(0,stopIndex) : names[index];
            UnityEngine.Debug.Log("Chose name " + chosenName);
        }
        else
            UnityEngine.Debug.Log($"Name not found for index {index}!");
    }
    public void setWidth()
    {
        this.width = (int)widthSlider.value;
        widthLabel.text = "Width: " + this.width;
    }
    public void setHeight()
    {
        this.height = (int)heightSlider.value;
        heightLabel.text = "Height: " + this.height;
    }
    public void setAttempts()
    {
        this.maxAttempts = (int)attemptsSlider.value;
        attemptsLabel.text = "Attempts: " + this.maxAttempts;
    }

    public void startCollapse()
    {
        Stopwatch sw = Stopwatch.StartNew();

        System.Random random = new System.Random();
        XDocument xdoc = XDocument.Parse(am.tilesetData.text);

        foreach (XElement xelem in xdoc.Root.Elements("simpletiled"))
        {
            WFCModel model;
            string name = xelem.Get<string>("name");
            Tileset tileset;
            if (name != chosenName || !am.tilesetLookup.TryGetValue(name, out tileset))
                continue;
            UnityEngine.Debug.Log($"< {name}");

            bool periodic = xelem.Get("periodic", false);
            string heuristicString = xelem.Get<string>("heuristic");
            var heuristic = heuristicString == "Scanline" ? WFCModel.Heuristic.Scanline : (heuristicString == "MRV" ? WFCModel.Heuristic.MRV : WFCModel.Heuristic.Entropy);

            string subset = xelem.Get<string>("subset");
            bool blackBackground = xelem.Get("blackBackground", false);
            model = new WaveFunctionCollapse(tileset, subset, width, height, periodic, blackBackground, heuristic, this);

            for (int k = 0; k < 10; k++)
            {
                Console.Write("> ");
                int seed = random.Next();
                bool success = model.Run(seed, xelem.Get("limit", -1));
                if (success)
                {
                    UnityEngine.Debug.Log("DONE");
                    model.Save($"WFC/output/{name} {seed}.png");
                    /*if (model is WaveFunctionCollapse stmodel && xelem.Get("textOutput", false))
                        System.IO.File.WriteAllText($"WFC/output/{name} {seed}.txt", stmodel.TextOutput());*/
                    break;
                }
                else UnityEngine.Debug.Log("CONTRADICTION");
            }
        }

        UnityEngine.Debug.Log($"time = {sw.ElapsedMilliseconds}");
    }

    private void readNameOptions()
    {
        names = new List<string>();
        XDocument xdoc = XDocument.Parse(am.tilesetData.text);
        foreach (string tilesetName in am.tilesetLookup.Keys)
        {
            bool set = false;
            foreach (XElement xelem in xdoc.Root.Elements("simpletiled"))
            {
                string current = tilesetName;
                string foundName = xelem.Get<string>("name");
                if (foundName == null || foundName != tilesetName)
                    continue;

                string subset = xelem.Get<string>("subset");
                string heuristic = xelem.Get<string>("heuristic");
                bool periodic = xelem.Get("periodic", false);
                bool blackBackground = xelem.Get("blackBackground", false);
                string addition = (subset != null ? subset : "");
                addition += periodic ? ((subset == null ? "" : ", ") + "periodic") : "";
                addition += heuristic != null ? ((!periodic ? "" : ", ") + heuristic) : "";
                addition += blackBackground ? ((heuristic == null ? "" : ", ") + "black background") : "";
                current += addition == "" ? "" : (": " + addition);

                names.Add(current);
                set = true;
            }

            if (!set)
                names.Add(tilesetName);
        }

        /* Deprecated reading from file (not possible with WebGL)
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
        }*/
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
