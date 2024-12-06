using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProceduralTerrain : MonoBehaviour
{
    public Texture2D hMap;

    private TerrainData terrainData;
    private Terrain terrain;
    private float terrainWidth;
    private float terrainHeight;
    private Texture2D gradientTexture;
    private int randomTiles;
    private bool corruption = false;

    [SerializeField] private GameObject[] grass;
    [SerializeField] private GameObject[] tree;
    [SerializeField] private GameObject[] rock;
    [SerializeField] private GameObject parent;
    [SerializeField] private Gradient terrainGradient;
    [SerializeField] private Material mat;
    [SerializeField] private GameObject water;
    [SerializeField] private GameObject sun;
    [SerializeField] private GameObject copyToObject;

    private ObjectPool grassPool;
    private ObjectPool treePool;
    private ObjectPool rockPool;

    private bool DoRotation = false;

    void Start()
    {
        grassPool = new ObjectPool(grass);
        treePool = new ObjectPool(tree);
        rockPool = new ObjectPool(rock);

        GenerateTerrain();
    }

    public void GenerateTerrain()
    {
        int randomCorruption = Random.Range(0, 2);
        if (randomCorruption == 1)
        {
            corruption = true;
            Debug.Log("Corruption is true");
        }

        hMap = new Texture2D(256, 256);
        hMap.LoadImage(File.ReadAllBytes(Path.Combine(Application.persistentDataPath, "noise.png")));
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        float[,] heights = new float[hMap.width, hMap.height];
        for (int y = 0; y < hMap.height; y++)
        {
            for (int x = 0; x < hMap.width; x++)
            {
                heights[x, y] = hMap.GetPixel(x, y).grayscale;
            }
        }

        if (corruption)
        {
            randomTiles = Random.Range(0, heights.Length - 25856 - 2560);
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    if (i > 5 && j > 5 && i < 15 && j < 15)
                    {
                        heights[randomTiles / 256 + i, randomTiles / 256 + j] -= 0.05f;
                    }
                    else
                    {
                        heights[randomTiles / 256 + i, randomTiles / 256 + j] += -0.03f;
                    }
                }
            }
        }

        terrainData.size = new Vector3(hMap.width, 100, hMap.height);
        terrainData.SetHeights(0, 0, heights);
        terrainWidth = terrainData.size.x;
        terrainHeight = terrainData.size.z;

        water.SetActive(true);
        sun.transform.rotation = Quaternion.Euler(Random.Range(50, -5), Random.Range(50, -50), 0);

        StartPlacingObjects();

        mat.SetTexture("terrainGradient", gradientTexture);

        mat.SetFloat("minHeight", 0);
        mat.SetFloat("maxHeight", terrainHeight);

        CopyToObject();
    }

    public IEnumerator PlaceObjectCoroutine()
    {
        int maxObjectsPerFrame = 50;
        int objectsInstantiated = 0;

        for (int x = 0; x < terrainWidth; x++)
        {
            for (int z = 0; z < terrainHeight; z++)
            {
                if (randomTiles / 256 == x && randomTiles / 256 == z && corruption)
                {
                    x += 20;
                    z += 20;
                    continue;
                }

                if (Fitness(hMap, x, z, 30, 0, 50, 0, -5, 0.25f) > 0.5)
                {
                    Vector3 pos = new Vector3(x + Random.Range(-0.5f, 0.5f), 0, z + Random.Range(-0.5f, 0.5f));
                    pos.y = terrain.SampleHeight(new Vector3(x, 0, z));
                    int index = Random.Range(0, grass.Length);
                    grassPool.GetObject(pos, Quaternion.identity, parent.transform, DoRotation);
                    objectsInstantiated++;
                }

                if (Fitness(hMap, x, z, 10000, 50, 50, 0, -20, 0) > 0.5)
                {
                    Vector3 pos = new Vector3(x + Random.Range(-0.5f, 0.5f), 0, z + Random.Range(-0.5f, 0.5f));
                    pos.y = terrain.SampleHeight(new Vector3(x, 0, z));
                    int index = Random.Range(0, tree.Length);
                    treePool.GetObject(pos, Quaternion.identity, parent.transform, DoRotation);
                    objectsInstantiated++;
                }

                if (Fitness(hMap, x, z, 100, 50, 10000, 50, -10, 0.25f) > 0.5)
                {
                    Vector3 pos = new Vector3(x + Random.Range(-0.5f, 0.5f), 0, z + Random.Range(-0.5f, 0.5f));
                    pos.y = terrain.SampleHeight(new Vector3(x, 0, z));
                    int index = Random.Range(0, rock.Length);
                    rockPool.GetObject(pos, Quaternion.identity, parent.transform, DoRotation);
                    objectsInstantiated++;
                }

                if (objectsInstantiated >= maxObjectsPerFrame)
                {
                    yield return null;
                    objectsInstantiated = 0;
                }
            }
        }

        GradientToTexture();
    }

    public void StartPlacingObjects()
    {
        StopAllCoroutines();
        StartCoroutine(PlaceObjectCoroutine());
    }

    private float Fitness(Texture2D noiseMapTexture, int x, int z, int maxH, int minH, int maxS, int minS, int minR, float maxR)
    {
        float fitness = 1;

        float steepness = terrainData.GetSteepness(x / terrainWidth, z / terrainHeight);
        if (steepness > maxS || steepness < minS)
        {
            fitness -= 0.7f;
        }

        float height = terrainData.GetHeight(x, z);
        if (height > maxH || height < minH)
        {
            fitness -= 0.7f;
        }

        fitness += Random.Range(minR, maxR);

        return fitness;
    }

    private void GradientToTexture()
    {
        gradientTexture = new Texture2D(1, 100);
        Color[] colors = new Color[100];

        for (int i = 0; i < 100; i++)
        {
            colors[i] = terrainGradient.Evaluate(i / 100.0f);
        }

        gradientTexture.SetPixels(colors);
        gradientTexture.Apply();
    }

    private void CopyToObject()
    {
        var bounds = terrain.terrainData.bounds;

        var mf = copyToObject.GetComponent<MeshFilter>();
        var m = mf.mesh;
        List<Vector3> vertices = new List<Vector3>();
        foreach (var vert in m.vertices)
        {
            var wPos = copyToObject.transform.localToWorldMatrix * vert;
            var newVert = vert;
            newVert.y = terrain.SampleHeight(wPos);
            vertices.Add(newVert);
        }
        m.SetVertices(vertices.ToArray());
        m.RecalculateNormals();
        m.RecalculateTangents();
        m.RecalculateBounds();
    }

    public void SetRotation()
    {
        DoRotation = !DoRotation;
    }
}

public class ObjectPool
{
    private GameObject[] prefabs;
    private Queue<GameObject> pool;
    private GameObject parent = new GameObject("Pool");

    private bool DoRotation = false;

    public ObjectPool(GameObject[] prefabs)
    {
        parent.SetActive(false);
        this.prefabs = prefabs;
        pool = new Queue<GameObject>();
        InitializePool();
    }

    private void InitializePool()
    {
        foreach (var prefab in prefabs)
        {
            GameObject obj = Object.Instantiate(prefab, parent.transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetObject(Vector3 position, Quaternion rotation, Transform parent, bool change)
    {
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = Object.Instantiate(prefabs[Random.Range(0, prefabs.Length)], position, rotation, parent);
        }
        obj.transform.DOScale(0, 0);
        obj.transform.DOScale(1, 3);
        if (change)
        {
            obj.transform.rotation = rotation;
        }
        else
        {
            RaycastHit Rhit;
            if (Physics.Raycast(obj.transform.position, -Vector3.up, out Rhit, 1000))
            {
                Vector3 terrainNormal = Rhit.normal;
                Vector3 objectRight = obj.transform.right;
                Vector3 objectForward = Vector3.Cross(objectRight, terrainNormal);
                obj.transform.rotation = Quaternion.LookRotation(objectForward, terrainNormal);
            }
        }

        obj.transform.position = position;
        obj.SetActive(true);
        return obj;
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}