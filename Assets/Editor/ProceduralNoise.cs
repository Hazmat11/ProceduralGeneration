using System.IO;
using UnityEditor;
using UnityEngine;

public class ProceduralNoise : EditorWindow
{
    public int width = 256;
    public int height = 256;
    public float noiseScale = 1;
    private float offsetX = 0f;
    private float offsetY = 0f;
    private Texture2D texture;


    [MenuItem("Tools/Generate Noise Texture")]

    public static void ShowWindow()
    {
        GetWindow<ProceduralNoise>("Noise Texture Generator");
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        texture = EditorGUILayout.ObjectField("Texture", texture, typeof(Texture2D), false) as Texture2D;
        if (GUILayout.Button("Generate", GUILayout.Width(100)))
        {
            texture = GenerateTexture(noiseScale);
        }
        EditorGUILayout.EndHorizontal();
        noiseScale = EditorGUILayout.Slider("Noise Scale", noiseScale, 0.1f, 10f);
    }

    private void OnEnable()
    {
        GenerateTexture(noiseScale);
    }

    public Texture2D GenerateTexture(float scale)
    {
        texture = new Texture2D(width, height);
        offsetX = Random.Range(0f, 50);
        offsetY = Random.Range(0f, 50);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = CalculateColor(x, y, scale);
                texture.SetPixel(x, y, color);
            }
        }

        string path = Path.Combine(Application.persistentDataPath, "noise.png");

        if (File.Exists(path))
            File.Delete(path);

        File.WriteAllBytes(path, texture.EncodeToPNG());

        AssetDatabase.Refresh();

        string assetPath = "Assets/noise.png";
        File.Copy(path, assetPath, true);

        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

        return texture;
    }

    Color CalculateColor(int x, int y, float scale)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale + offsetY; 
        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        return new Color(sample, sample, sample);
    }
}
