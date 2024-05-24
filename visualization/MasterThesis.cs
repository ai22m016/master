using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public enum BIOMES
{
    blob, cross, grid, line
}

public enum GOALS
{
    ALL, DEFEND, DESTROY, ESCAPE
}

public class MasterThesis : MonoBehaviour
{
    public BIOMES biome;
    List<GameObject> hexes = new List<GameObject>();
    GOALS goal;

    public void GoalLeft()
    {// Used to choose a different goal to display
        goal = goal - 1 >= 0 ? goal - 1 : GOALS.ESCAPE;
        Display();
    }

    public void GoalRight()
    {// Used to choose a different goal to display
        goal = goal + 1 <= GOALS.ESCAPE ? goal + 1 : GOALS.ALL;
        Display();
    }

    public void GoLeft()
    {
        biome = biome - 1 >= 0 ? biome - 1 : BIOMES.line;
        Display();
    }

    public void GoRight()
    {
        biome = biome + 1 <= BIOMES.line ? biome + 1 : BIOMES.blob;
        Display();
    }

    int pcgIndex = 0;
    public void DisplayPCG()
    {
        while (hexes.Count > 0)
        {
            Destroy(hexes[0]);
            hexes.RemoveAt(0);
        }

        string[] files = System.IO.Directory.GetFiles("C:\\Users\\Mulder\\AppData\\LocalLow\\FreeRabbitGames\\Hexer\\master\\pcg\\", "*.CSV");

        List<Vector3> positions = new List<Vector3>();

        if (pcgIndex >= files.Length)
            pcgIndex = 0;

        int aa = 1;
        foreach (string file in files)
        {
            string[] tokens = file.Split("\\");

            Debug.Log(tokens[tokens.Length - 1]);
            if (tokens[tokens.Length - 1] != pcgIndex + "aa.CSV")
                continue;

            Debug.Log("Opening file: " + file);
            using (var reader = new StreamReader(file))
            {
                List<double> listX = new List<double>();
                List<double> listY = new List<double>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    float x = float.Parse(values[0]);
                    float y = float.Parse(values[1]);

                    Vector3 newPos = new Vector3(x, y, 0);

                    bool found = false;
                    for (int n = 0; n < positions.Count; ++n)
                    {
                        if (GetAbsDistance(newPos, positions[n]) < 0.1)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        positions.Add(newPos);
                    }
                }
            }
        }

        Debug.Log("Found " + positions.Count + " hexes");

        // Positions now holds all hex positions that have been used
        // Counts holds the number of times these positions have been used
        ColorGradient cg = new ColorGradient();
        for (int n = 0; n < positions.Count; ++n)
        {
            GameObject hex = Instantiate(Resources.Load<GameObject>("HexTile"));
            hexes.Add(hex);
            positions[n] += new Vector3(-100,-100, 100);
            hex.transform.localScale *= 2f;
            hex.transform.position = positions[n];
            hex.transform.rotation = Quaternion.Euler(90, 0, 0);
            hex.GetComponent<MeshRenderer>().material = Resources.Load<Material>("MasterThesisHeatmap");
            hex.GetComponent<MeshRenderer>().material.color = Color.black;
        }


    }

    internal void Display()
    {
        while(hexes.Count > 0)
        {
            Destroy(hexes[0]);
            hexes.RemoveAt(0);
        }

        string[] files = System.IO.Directory.GetFiles("C:\\Users\\Mulder\\AppData\\LocalLow\\FreeRabbitGames\\Hexer\\master\\" + biome.ToString() + "/", "*.CSV");

        List<Vector3> positions = new List<Vector3>();
        List<int> counts = new List<int>();

        int defend = 0, destroy = 0, escape = 0;

        foreach (string file in files)
        {
            if (goal == GOALS.ESCAPE && !file.Contains("escape"))
                continue;
            if (goal == GOALS.DEFEND && !file.Contains("defend"))
                continue;
            if (goal == GOALS.DESTROY && !file.Contains("destroy"))
                continue;

            if (file.Contains("destroy"))
                ++destroy;
            else if (file.Contains("defend"))
                ++defend;
            else
                ++escape;

            Debug.Log("Opening file: " + file);
            using (var reader = new StreamReader(file))
            {
                List<double> listX = new List<double>();
                List<double> listY = new List<double>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    float x = float.Parse(values[0]);
                    float y = float.Parse(values[1]);

                    Vector3 newPos = new Vector3(x, y, 0);

                    bool found = false;
                    for (int n = 0; n < positions.Count; ++n)
                    {
                        if (GetAbsDistance(newPos, positions[n]) < 0.1)
                        {
                            counts[n]++;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        positions.Add(newPos);
                        counts.Add(1);
                    }
                }
            }
        }

        Debug.Log("Found " + positions.Count + " hexes");

        // Find max count
        int maxCount = -999;
        foreach (int count in counts)
        {
            if (count > maxCount)
                maxCount = count;
        }

        // Positions now holds all hex positions that have been used
        // Counts holds the number of times these positions have been used
        ColorGradient cg = new ColorGradient();
        for (int n = 0; n < positions.Count; ++n)
        {
            GameObject hex = Instantiate(Resources.Load<GameObject>("HexTile"));
            hexes.Add(hex);
            hex.transform.position = positions[n];
            hex.transform.rotation = Quaternion.Euler(90, 0, 0);
            hex.GetComponent<MeshRenderer>().material = Resources.Load<Material>("MasterThesisHeatmap");
            hex.GetComponent<MeshRenderer>().material.color = cg.getColorAtValue(counts[n] / (float)maxCount);
        }


        int sum = defend + destroy + escape;
        float percentage = defend / (float)sum * 10000;
        int buffer = (int)percentage;
        percentage = buffer / (float)100;
        GameObject.Find("Defend").GetComponent<TextMeshProUGUI>().text = "Defend: " + defend + "   " + percentage + " %";
        percentage = destroy / (float)sum * 10000;
        buffer = (int)percentage;
        percentage = buffer / (float)100;
        GameObject.Find("Destroy").GetComponent<TextMeshProUGUI>().text = "Destroy: " + destroy + "   " + percentage + " %";
        percentage = escape / (float)sum * 10000;
        buffer = (int)percentage;
        percentage = buffer / (float)100;
        GameObject.Find("Escape").GetComponent<TextMeshProUGUI>().text = "Escape: " + escape + "   " + percentage + " %";
        GameObject.Find("Total").GetComponent<TextMeshProUGUI>().text = "Total: " + sum;
        GameObject.Find("Biome").GetComponent<TextMeshProUGUI>().text = biome.ToString();
        GameObject.Find("Goal").GetComponent<TextMeshProUGUI>().text = goal.ToString();

    }

    // Start is called before the first frame update
    void Start()
    {
        DisplayPCG();
    }

    Color getValueBetweenTwoFixedColors(float value)
    {
        Debug.Log(value);
        int aR = 0; int aG = 0; int aB = 255;  // RGB for our 1st color (blue in this case).
        int bR = 255; int bG = 0; int bB = 0;    // RGB for our 2nd color (red in this case).

        float red = (float)(bR - aR) * value + aR;      // Evaluated as -255*value + 255.
        float green = (float)(bG - aG) * value + aG;      // Evaluates as 0.
        float blue = (float)(bB - aB) * value + aB;      // Evaluates as 255*value + 0.

        return new Color(red, green, blue, 1);
    }

    internal float GetAbsDistance(Vector3 from, Vector3 to)
    {
        return Mathf.Abs(Vector3.Distance(from, to));
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.E))
        {
            Vector3 pos = Camera.main.transform.position;
            pos.z += Time.deltaTime * 25;
            Camera.main.transform.position = pos;
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            Vector3 pos = Camera.main.transform.position;
            pos.z -= Time.deltaTime * 25;
            Camera.main.transform.position = pos;
        }

        if (Input.GetKey(KeyCode.W))
        {
            Vector3 pos = Camera.main.transform.position;
            pos.y += Time.deltaTime * 25;
            Camera.main.transform.position = pos;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Vector3 pos = Camera.main.transform.position;
            pos.y -= Time.deltaTime * 25;
            Camera.main.transform.position = pos;
        }

        if (Input.GetKey(KeyCode.A))
        {
            Vector3 pos = Camera.main.transform.position;
            pos.x -= Time.deltaTime * 25;
            Camera.main.transform.position = pos;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Vector3 pos = Camera.main.transform.position;
            pos.x += Time.deltaTime * 25;
            Camera.main.transform.position = pos;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            pcgIndex++;
            DisplayPCG();
        }
    }
}


public struct ColorPoint  // Internal class used to store colors at different points in the gradient.
{
    public float r, g, b;      // Red, green and blue values of our color.
    public float val;        // Position of our color along the gradient (between 0 and 1).
    public ColorPoint(float red, float green, float blue, float value)
    {
        r = red; g = green; b = blue; val = value;
    }
};

// Taken from
// https://www.andrewnoske.com/wiki/Code_-_heatmaps_and_color_gradients
public class ColorGradient
{
    List<ColorPoint> color = new List<ColorPoint>();      // An array of color points in ascending value.

    public ColorGradient() { createDefaultHeatMapGradient(); }

    //-- Inserts a new color point into its correct position:
    void addColorPoint(float red, float green, float blue, float value)
    {
        for (int i = 0; i < color.Count; i++)
        {
            if (value < color[i].val)
            {
                color.Insert(i, new ColorPoint(red, green, blue, value));
                return;
            }
        }
        color.Add(new ColorPoint(red, green, blue, value));
    }

    //-- Places a 5 color heapmap gradient into the "color" vector:
    void createDefaultHeatMapGradient()
    {
        color.Clear();
        color.Add(new ColorPoint(0, 0, 1, 0.0f));      // Blue.
        color.Add(new ColorPoint(0, 1, 1, 0.25f));     // Cyan.
        color.Add(new ColorPoint(0, 1, 0, 0.5f));      // Green.
        color.Add(new ColorPoint(1, 1, 0, 0.75f));     // Yellow.
        color.Add(new ColorPoint(1, 0, 0, 1.0f));      // Red.
    }

    //-- Inputs a (value) between 0 and 1 and outputs the (red), (green) and (blue)
    //-- values representing that position in the gradient.
    public Color getColorAtValue(float value)
    {
        if (color.Count == 0)
            return Color.black;

        float red, green, blue;
        for (int i = 0; i < color.Count; i++)
        {
            if (value < color[i].val)
            {
                if (i > 0)
                {
                    ColorPoint prevColor = color[i - 1];
                    float valueDiff = (prevColor.val - color[i].val);
                    float fractBetween = (valueDiff == 0) ? 0 : (value - color[i].val) / valueDiff;
                    red = (prevColor.r - color[i].r) * fractBetween + color[i].r;
                    green = (prevColor.g - color[i].g) * fractBetween + color[i].g;
                    blue = (prevColor.b - color[i].b) * fractBetween + color[i].b;
                    return new Color(red, green, blue, 1);
                }
            }
        }
        red = color[color.Count - 1].r;
        green = color[color.Count - 1].g;
        blue = color[color.Count - 1].b;
        return new Color(red, green, blue, 1);
    }
}
