using sc.terrain.proceduralpainter;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Generate : MonoBehaviour
{

    [SerializeField] GameObject terrain;
    [SerializeField] GameObject parent;

    public int width = 256;
    public int height = 256;
    public float noiseScale = 1;
    private float offsetX = 0f;
    private float offsetY = 0f;
    private Texture2D texture;

    public Slider slider;
    void Start()
    {
        DoGeneration();
    }

    public void DoGeneration()
    {
        GenerateTexture(noiseScale);
        foreach (Transform child in parent.transform)
        {
            Destroy(child.gameObject);
        }
        terrain.GetComponent<ProceduralTerrain>().GenerateTerrain();
    }

    public void ChangeValue()
    {
        noiseScale = slider.value;
        slider.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = noiseScale.ToString();
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

        string assetPath = "Assets/noise.png";
        File.Copy(path, assetPath, true);

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
