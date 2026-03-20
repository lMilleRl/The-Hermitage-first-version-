using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerateManager : MonoBehaviour
{
    //[SerializeField] private Transform _playerPos;

    //private ChunksGenerator _chunksGenerator;

    //private ObjectsGenerateManager _objectsGenerateManager;

    //[SerializeField] private PerlinNoise perlin;
    //public PerlinNoiseData perlinData;
    //private LODChunksManager[] LodLevels;

    //[SerializeField] private GameObject player;
    //[SerializeField] private PlayerData playerData;

    //private int seed;
    //private int oldSeed;

    //private List<ChunkData> Chunks;
    //private List<int> ChunksToMove;
    //private HashSet<Vector2Int> ChunksPos;
    //[SerializeField] private GameObject chunkPrefab;


    //[Range(3, 70)][SerializeField] private int RadiusChunks;
    //private int OldRadius;

    //private int sqrRadiusChunks;
    //private int sqrRadiusChunksInequality;

    //[Range(1, 5)][SerializeField] private int CountOriginalChunksInLengthBatch;

    //[Range(10, 25)][SerializeField] private int CountViewOriginalChunksInLengthBatch;


    //[SerializeField] private float polygonScale;

    //private bool isNewGame;

    //private ObjectsGenerateManager ObjectsGenManager;

    //public void Start()
    //{
    //    LodLevels = GetComponents<LODChunksManager>();

    //    var TempForSort = new List<LODChunksManager>(LodLevels);
    //    TempForSort.Sort((a, b) => b.LodDensity.CompareTo(a.LodDensity));
    //    LodLevels = TempForSort.ToArray();

    //    LodLevels[0].RadiusToDontDrawBatches = 0;
    //    LodLevels[0].RadiusLodBatches = LodLevels[0].InitRadiusLodBatches + LodLevels[0].RadiusToDontDrawBatches;

    //    for (int i = 1; i < LodLevels.Length; i++)
    //    {
    //        LodLevels[i].RadiusToDontDrawBatches = LodLevels[i - 1].RadiusLodBatches - 1;
    //        LodLevels[i].RadiusLodBatches = LodLevels[i].InitRadiusLodBatches + LodLevels[i].RadiusToDontDrawBatches + 1;
    //    }


    //    ObjectsGenManager = GetComponent<ObjectsGenerateManager>();

    //    perlin = GetComponent<PerlinNoise>();

    //    perlinData.LoadBaseConstantData();

    //    SaveSystem.SavedData<int> seedData = SaveSystem.LoadData<int>("Seed");
    //    if (seedData == null)
    //    {
    //        var rand = new Unity.Mathematics.Random();
    //        seed = rand.NextInt();
    //    }
    //    else if (seedData.data == 0) seed = Int32.MinValue;
    //    else
    //        seed = seedData.data;

    //    SaveSystem.SavedData<int> oldSeedData = SaveSystem.LoadData<int>("OldSeed");
    //    if (oldSeedData == null)
    //        oldSeed = seed + 1;
    //    else
    //        oldSeed = oldSeedData.data;

    //    Debug.Log($"seed {seed}");
    //    Debug.Log($"oldSeed {oldSeed}");


    //    if (seed != oldSeed || perlinData.links == null || perlinData.settingsOctaves == null || perlinData.rivers == null || perlinData.reliefTypes == null)
    //    {
    //        isNewGame = true;
    //        SaveSystem.TryDeleteData(playerData.nameFilePlayerPos);
    //        Time.timeScale = 0;

    //        _ = CreateGenerationData();
    //    }
    //    else
    //    {
    //        isNewGame = false;
    //        perlin.SetStartingData(seed);
    //        perlin.InitNativeBigBaseData();
    //        perlin.SetStartCoordHeight();

    //        ObjectsGenManager.SetBaseObjectsInStart(isNewGame);
    //        SaveSystem.SavedData<Vector3> dataPlayerPos = SaveSystem.LoadData<Vector3>(playerData.nameFilePlayerPos);
    //        if (dataPlayerPos != null)
    //        {
    //            Vector3 startPlayerPos = dataPlayerPos.data;
    //            currentPlayerChunk = new Vector2Int((int)startPlayerPos.x / (int)((perlinData.sizeChunk - 1) * CountOriginalChunksInLengthBatch * polygonScale), (int)startPlayerPos.z / (int)((perlinData.sizeChunk - 1) * CountOriginalChunksInLengthBatch * polygonScale));
    //            currentPosViewChunks = new Vector2Int((int)startPlayerPos.x / (int)((perlinData.sizeChunk - 1) * CountViewOriginalChunksInLengthBatch * polygonScale), (int)startPlayerPos.z / (int)((perlinData.sizeChunk - 1) * CountViewOriginalChunksInLengthBatch * polygonScale));

    //            for (int i = 0; i < LodLevels.Length; i++)
    //            {
    //                LodLevels[i].currentPlayerChunk = currentPosViewChunks;
    //            }
    //        }
    //        else
    //        {
    //            currentPlayerChunk = new Vector2Int(0, 0);
    //            currentPosViewChunks = new Vector2Int(0, 0);

    //            for (int i = 0; i < LodLevels.Length; i++)
    //            {
    //                LodLevels[i].currentPlayerChunk = currentPosViewChunks;
    //            }
    //        }

    //        CreateViewRangeChunks();

    //        for (int i = 0; i < LodLevels.Length; i++)
    //        {
    //            LodLevels[i].CreateViewRangeChunks();
    //        }

    //        Time.timeScale = 0;
    //        StartCoroutine(WaitingEndGenerationChunks());

    //        for (int i = 0; i < LodLevels.Length; i++)
    //        {
    //            LodLevels[i].isCheckSetGenerate = true;
    //        }
    //        isCheckSetGenerate = true;
    //        //GetComponent<TestPerlinNoise>().perlin = perlin;
    //        //GetComponent<TestPerlinNoise>().StartTest();
    //    }
    //}

    //private async Task CreateGenerationData()
    //{
    //    Debug.Log("startCrerateData");

    //    (perlinData.links, perlinData.settingsOctaves, perlinData.rivers) = (null, null, null);

    //    (perlinData.links, perlinData.settingsOctaves, perlinData.rivers) = await Task.Run(() =>
    //    {
    //        try
    //        {
    //            return perlin.InitBigBaseData(seed);
    //        }
    //        catch (Exception ex)
    //        {
    //            Debug.LogError($"Error during InitBigBaseData: {ex.Message}");
    //            return (null, null, null); // Возвращаем пустые данные в случае ошибки
    //        }
    //    });

    //    perlin.InitNativeBigBaseData();
    //    perlin.SetStartCoordHeight();

    //    Debug.Log("endCreateData");
    //    oldSeed = seed;

    //    ObjectsGenManager.SetBaseObjectsInStart(isNewGame);

    //    currentPlayerChunk = new Vector2Int(0, 0);
    //    currentPosViewChunks = new Vector2Int(0, 0);

    //    for (int i = 0; i < LodLevels.Length; i++)
    //    {
    //        LodLevels[i].currentPlayerChunk = currentPosViewChunks;
    //    }

    //    try
    //    {
    //        CreateViewRangeChunks();

    //        for (int i = 0; i < LodLevels.Length; i++)
    //        {
    //            LodLevels[i].CreateViewRangeChunks();
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogError($"Error during CreateViewRangeChunks: {ex.Message}");
    //    }

    //    StartCoroutine(WaitingEndGenerationChunks());

    //    for (int i = 0; i < LodLevels.Length; i++)
    //    {
    //        LodLevels[i].isCheckSetGenerate = true;
    //    }
    //    isCheckSetGenerate = true;
    //    //GetComponent<TestPerlinNoise>().perlin = perlin;
    //    //GetComponent<TestPerlinNoise>().StartTest();
    //}

    //[Range(1, 16)]
    //[SerializeField] private int LodDensity;

    //private uint[] NormalsLodTriangles;
    //private uint[] LodTriangles;

    //public void InitLodTrianlges()
    //{
    //    LodTriangles = new uint[CountOriginalChunksInLengthBatch * CountOriginalChunksInLengthBatch * 6 * LodDensity * LodDensity];

    //    int index = 0;
    //    for (uint y = 0; y < CountOriginalChunksInLengthBatch * LodDensity; y++)
    //    {
    //        for (uint x = 0; x < CountOriginalChunksInLengthBatch * LodDensity; x++)
    //        {
    //            uint topLeft = (uint)(x + y * (CountOriginalChunksInLengthBatch * LodDensity + 1));
    //            uint bottomLeft = (uint)(x + (y + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1));
    //            uint topRight = topLeft + 1;
    //            uint bottomRight = bottomLeft + 1;

    //            // Первый треугольник
    //            LodTriangles[index++] = topLeft;
    //            LodTriangles[index++] = bottomLeft;
    //            LodTriangles[index++] = topRight;

    //            // Второй треугольник
    //            LodTriangles[index++] = topRight;
    //            LodTriangles[index++] = bottomLeft;
    //            LodTriangles[index++] = bottomRight;
    //        }
    //    }
    //    NormalsLodTriangles = new uint[(CountOriginalChunksInLengthBatch * LodDensity + 2) * (CountOriginalChunksInLengthBatch * LodDensity + 2) * 6];

    //    index = 0;
    //    for (uint y = 0; y < CountOriginalChunksInLengthBatch * LodDensity + 2; y++)
    //    {
    //        for (uint x = 0; x < CountOriginalChunksInLengthBatch * LodDensity + 2; x++)
    //        {
    //            uint topLeft = (uint)(x + y * (CountOriginalChunksInLengthBatch * LodDensity + 3));
    //            uint bottomLeft = (uint)(x + (y + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 3));
    //            uint topRight = topLeft + 1;
    //            uint bottomRight = bottomLeft + 1;

    //            // Первый треугольник
    //            NormalsLodTriangles[index++] = topLeft;
    //            NormalsLodTriangles[index++] = bottomLeft;
    //            NormalsLodTriangles[index++] = topRight;

    //            // Второй треугольник
    //            NormalsLodTriangles[index++] = topRight;
    //            NormalsLodTriangles[index++] = bottomLeft;
    //            NormalsLodTriangles[index++] = bottomRight;
    //        }
    //    }
    //}
}
