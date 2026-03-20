using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;


public class ChunksGenerator : MonoBehaviour
{
    [SerializeField][HideInInspector] private PerlinNoise perlin;
    public PerlinNoiseData perlinData;
    private LODChunksManager[] LodLevels;

    [SerializeField] private GameObject player;
    [SerializeField] private PlayerData playerData;

    private int seed;
    private int oldSeed;
        
    private List<ChunkData> Chunks;    
    private List<int> ChunksToMove;
    private HashSet<Vector2Int> ChunksPos;
    [SerializeField] private GameObject chunkPrefab;
        
    [Range(3, 70)][SerializeField] private int RadiusChunks;
    private int OldRadius;

    private int sqrRadiusChunks;
    private int sqrRadiusChunksInequality;

    [Range(1, 5)][SerializeField] private int CountOriginalChunksInLengthBatch;        

    [Range(10, 25)][SerializeField] private int CountViewOriginalChunksInLengthBatch;


    [SerializeField] private float polygonScale;
    
    private Vector2Int currentPlayerChunk;
    private Vector2 playerChunk;    

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct GeneratedMeshVertex
    {
        public Vector3 pos;
        public Vector3 normal;
        public ushort uvX, uvY;

        public GeneratedMeshVertex(Vector3 pos, Vector3 normal, ushort uvX, ushort uvY)
        {
            this.pos = pos;
            this.normal = normal;
            this.uvX = uvX;
            this.uvY = uvY;
        }
    }

    private bool isNewGame;

    private ObjectsGenerateManager ObjectsGenManager;

    public void Start()
    {
        LodLevels = GetComponents<LODChunksManager>();
        
        var TempForSort = new List<LODChunksManager>(LodLevels);
        TempForSort.Sort((a, b) => b.LodDensity.CompareTo(a.LodDensity));
        LodLevels = TempForSort.ToArray();

        LodLevels[0].RadiusToDontDrawBatches = 0;
        LodLevels[0].RadiusLodBatches = LodLevels[0].InitRadiusLodBatches + LodLevels[0].RadiusToDontDrawBatches;

        for (int i = 1; i < LodLevels.Length; i++)
        {
            LodLevels[i].RadiusToDontDrawBatches = LodLevels[i - 1].RadiusLodBatches - 1;
            LodLevels[i].RadiusLodBatches = LodLevels[i].InitRadiusLodBatches + LodLevels[i].RadiusToDontDrawBatches + 1;
        }
        

        ObjectsGenManager = GetComponent<ObjectsGenerateManager>();          

        perlin = GetComponent<PerlinNoise>();
        
        perlinData.LoadBaseConstantData();              

        SaveSystem.SavedData<int> seedData = SaveSystem.LoadData<int>("Seed");
        if(seedData == null)
        {
            var rand = new Unity.Mathematics.Random();
            seed = rand.NextInt();
        }            
        else if(seedData.data == 0) seed = Int32.MinValue;
        else
            seed = seedData.data;

        SaveSystem.SavedData<int> oldSeedData = SaveSystem.LoadData<int>("OldSeed");
        if (oldSeedData == null)
            oldSeed = seed + 1;
        else
            oldSeed = oldSeedData.data;        

        Debug.Log($"seed {seed}");
        Debug.Log($"oldSeed {oldSeed}");       
        
        
        if (seed != oldSeed || perlinData.links == null || perlinData.settingsOctaves == null || perlinData.rivers == null || perlinData.reliefTypes == null)
        {
            isNewGame = true;
            SaveSystem.TryDeleteData(playerData.nameFilePlayerPos);
            Time.timeScale = 0;

            _ = CreateGenerationData();            
        }            
        else
        {
            isNewGame = false;
            perlin.SetStartingData(seed);
            perlin.InitNativeBigBaseData();
            perlin.SetStartCoordHeight();            

            ObjectsGenManager.SetBaseObjectsInStart(isNewGame);
            SaveSystem.SavedData<Vector3> dataPlayerPos = SaveSystem.LoadData<Vector3>(playerData.nameFilePlayerPos);
            if (dataPlayerPos != null)
            {
                Vector3 startPlayerPos = dataPlayerPos.data;
                currentPlayerChunk = new Vector2Int((int)startPlayerPos.x / (int)((perlinData.sizeChunk - 1) * CountOriginalChunksInLengthBatch * polygonScale), (int)startPlayerPos.z / (int)((perlinData.sizeChunk - 1) * CountOriginalChunksInLengthBatch * polygonScale));
                currentPosViewChunks = new Vector2Int((int)startPlayerPos.x / (int)((perlinData.sizeChunk - 1) * CountViewOriginalChunksInLengthBatch * polygonScale), (int)startPlayerPos.z / (int)((perlinData.sizeChunk - 1) * CountViewOriginalChunksInLengthBatch * polygonScale));
                
                    for (int i = 0; i < LodLevels.Length; i++)
                    {
                        LodLevels[i].currentPlayerChunk = currentPosViewChunks;
                    }
            }
            else
            {                
                currentPlayerChunk = new Vector2Int(0, 0);
                currentPosViewChunks = new Vector2Int(0, 0);
                
                    for (int i = 0; i < LodLevels.Length; i++)
                    {
                        LodLevels[i].currentPlayerChunk = currentPosViewChunks;
                    }
            }

            CreateViewRangeChunks();
            
                for (int i = 0; i < LodLevels.Length; i++)
                {
                    LodLevels[i].CreateViewRangeChunks();
                }           

            Time.timeScale = 0;            
            StartCoroutine(WaitingEndGenerationChunks());
            
                for (int i = 0; i < LodLevels.Length; i++)
                {
                    LodLevels[i].isCheckSetGenerate = true;
                }
            isCheckSetGenerate = true;
            //GetComponent<TestPerlinNoise>().perlin = perlin;
            //GetComponent<TestPerlinNoise>().StartTest();
        }
    }
    
    private async Task CreateGenerationData()
    {
        Debug.Log("startCrerateData");        

        (perlinData.links, perlinData.settingsOctaves, perlinData.rivers) = (null, null, null);

        (perlinData.links, perlinData.settingsOctaves, perlinData.rivers) = await Task.Run(() =>
        {
            try
            {
                return perlin.InitBigBaseData(seed);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during InitBigBaseData: {ex.Message}");
                return (null, null, null); // Возвращаем пустые данные в случае ошибки
            }
        });
        
        perlin.InitNativeBigBaseData();
        perlin.SetStartCoordHeight();

        Debug.Log("endCreateData");
        oldSeed = seed;

        ObjectsGenManager.SetBaseObjectsInStart(isNewGame);

        currentPlayerChunk = new Vector2Int(0, 0);
        currentPosViewChunks = new Vector2Int(0, 0);
        
            for (int i = 0; i < LodLevels.Length; i++)
            {
                LodLevels[i].currentPlayerChunk = currentPosViewChunks;
            }

        try
        {
            CreateViewRangeChunks();
            
                for (int i = 0; i < LodLevels.Length; i++)
                {                    
                    LodLevels[i].CreateViewRangeChunks();
                }           
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during CreateViewRangeChunks: {ex.Message}");
        }
        
        StartCoroutine(WaitingEndGenerationChunks());
        
            for (int i = 0; i < LodLevels.Length; i++)
            {
                LodLevels[i].isCheckSetGenerate = true;
            }
        isCheckSetGenerate = true;
        //GetComponent<TestPerlinNoise>().perlin = perlin;
        //GetComponent<TestPerlinNoise>().StartTest();
    }

    [Range(1, 16)]
    [SerializeField] private int LodDensity;

    private uint[] NormalsLodTriangles;
    private uint[] LodTriangles;

    public void InitLodTrianlges()
    {
        LodTriangles = new uint[CountOriginalChunksInLengthBatch * CountOriginalChunksInLengthBatch * 6 * LodDensity * LodDensity];

        int index = 0;
        for (uint y = 0; y < CountOriginalChunksInLengthBatch * LodDensity; y++)
        {
            for (uint x = 0; x < CountOriginalChunksInLengthBatch * LodDensity; x++)
            {
                uint topLeft = (uint)(x + y * (CountOriginalChunksInLengthBatch * LodDensity + 1));
                uint bottomLeft = (uint)(x + (y + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1));
                uint topRight = topLeft + 1;
                uint bottomRight = bottomLeft + 1;

                // Первый треугольник
                LodTriangles[index++] = topLeft;
                LodTriangles[index++] = bottomLeft;
                LodTriangles[index++] = topRight;

                // Второй треугольник
                LodTriangles[index++] = topRight;
                LodTriangles[index++] = bottomLeft;
                LodTriangles[index++] = bottomRight;
            }
        }
        NormalsLodTriangles = new uint[(CountOriginalChunksInLengthBatch * LodDensity + 2) * (CountOriginalChunksInLengthBatch * LodDensity + 2) * 6];

        index = 0;
        for (uint y = 0; y < CountOriginalChunksInLengthBatch * LodDensity + 2; y++)
        {
            for (uint x = 0; x < CountOriginalChunksInLengthBatch * LodDensity + 2; x++)
            {
                uint topLeft = (uint)(x + y * (CountOriginalChunksInLengthBatch * LodDensity + 3));
                uint bottomLeft = (uint)(x + (y + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 3));
                uint topRight = topLeft + 1;
                uint bottomRight = bottomLeft + 1;

                // Первый треугольник
                NormalsLodTriangles[index++] = topLeft;
                NormalsLodTriangles[index++] = bottomLeft;
                NormalsLodTriangles[index++] = topRight;

                // Второй треугольник
                NormalsLodTriangles[index++] = topRight;
                NormalsLodTriangles[index++] = bottomLeft;
                NormalsLodTriangles[index++] = bottomRight;
            }
        }
    }


    [SerializeField] private GameObject LoadScreen;

    private bool IsChunksGenerated;
    private IEnumerator WaitingEndGenerationChunks()
    {
        while (!IsChunksGenerated)
        {            
            yield return null;
        }
        bool IsGenerated = false;
        
        {
            while (!IsGenerated)
            {                
                for (int i = 0; i < LodLevels.Length; i++)
                {
                    IsGenerated = LodLevels[i].IsBatchesGenerated;
                    if (LodLevels[i].IsBatchesGenerated == false) break;
                }
                yield return null;
            }
        }
        LoadScreen.SetActive(false);       

        Time.timeScale = 1;

        //var test = GetComponent<TestPerlinNoise>();
        //test.tex = new Texture2D(perlinData.settingsSize * 10, perlinData.settingsSize * 10, TextureFormat.ARGB32, false);
        //test.tex.filterMode = FilterMode.Point;
        //var color = new Color();
        //int arrX; int arrY;
        //for (int y = 0; y < perlinData.settingsSize * 10; y++)
        //{
        //    for (int x = 0; x < perlinData.settingsSize * 10; x++)
        //    {
        //        arrX = Mathf.FloorToInt(x / 10f); arrY = Mathf.FloorToInt(y / 10f);
        //        if (perlinData.reliefTypes[arrX + arrY * perlinData.settingsSize] == PerlinNoiseData.ReliefType.Forest)
        //        {
        //            color.r = 0;
        //            color.g = 1;
        //            color.b = 0;
        //            color.a = 1;
        //        }
        //        if (perlinData.reliefTypes[arrX + arrY * perlinData.settingsSize] == PerlinNoiseData.ReliefType.See)
        //        {
        //            color.r = 0;
        //            color.g = 0;
        //            color.b = 1;
        //            color.a = 1;
        //        }
        //        if (perlinData.reliefTypes[arrX + arrY * perlinData.settingsSize] == PerlinNoiseData.ReliefType.HighForest)
        //        {
        //            color.r = 1;
        //            color.g = 1;
        //            color.b = 1;
        //            color.a = 1;
        //        }
        //        if (perlinData.reliefTypes[arrX + arrY * perlinData.settingsSize] == PerlinNoiseData.ReliefType.Spawn)
        //        {
        //            color.r = 0;
        //            color.g = 0;
        //            color.b = 0;
        //            color.a = 1;
        //        }
        //        test.tex.SetPixel(x, y, color);
        //    }
        //}
        //test.tex.Apply();
    }

    private float ConvertBatchPosInWorldPos;    
    private void CreateViewRangeChunks()
    {   
        LodLevels = GetComponents<LODChunksManager>();
        
        {
            var TempForSort = new List<LODChunksManager>(LodLevels);
            TempForSort.Sort((a, b) => b.LodDensity.CompareTo(a.LodDensity));
            LodLevels = TempForSort.ToArray();

            LodLevels[0].RadiusToDontDrawBatches = 0;
            LodLevels[0].RadiusLodBatches = LodLevels[0].InitRadiusLodBatches + LodLevels[0].RadiusToDontDrawBatches;
            for (int i = 1; i < LodLevels.Length; i++)
            {
                LodLevels[i].RadiusToDontDrawBatches = LodLevels[i - 1].RadiusLodBatches - 1;
                LodLevels[i].RadiusLodBatches = LodLevels[i].InitRadiusLodBatches + LodLevels[i].RadiusToDontDrawBatches + 1;
            }
        }

        isCheckSetGenerate = false;
        IsChunksGenerated = false;

        
            for (int i = 0; i < LodLevels.Length; i++)
            {
                LodLevels[i].isCheckSetGenerate = false;
                LodLevels[i].IsBatchesGenerated = false;
                LodLevels[i].seed = seed;
                LodLevels[i].polygonScale = polygonScale;
                LodLevels[i].CountOriginalChunksInLengthBatch = CountViewOriginalChunksInLengthBatch;
            }

        InitLodTrianlges();

        OldRadius = RadiusChunks;
        ConvertBatchPosInWorldPos = CountOriginalChunksInLengthBatch * (perlinData.sizeChunk - 1) * polygonScale;
        sqrRadiusChunks = RadiusChunks * RadiusChunks;
        sqrRadiusChunksInequality = (RadiusChunks - 1) * (RadiusChunks - 1);

        ChunkGenerateQueue = new Queue<JobHandle>(sqrRadiusChunks * 4);
        ChunkGenerateJobQueue = new Queue<MeshChunkBuilder.GenerateLodMeshParallel>(sqrRadiusChunks * 4);        
        
        DistToChangePosSimulation = 0.5f + (float)(perlinData.sizeChunk - 1) / ConvertBatchPosInWorldPos;                        

        if (Chunks != null)
        {            
            for (int i = 0; i < Chunks.Count; i++)
            {
                if (Chunks[i].chunk != null) Destroy(Chunks[i].chunk);
            }            
        }                

        Chunks = new List<ChunkData>(sqrRadiusChunks * 4);
        ChunksToMove = new List<int>(sqrRadiusChunks * 4);
        ChunksPos = new HashSet<Vector2Int>(sqrRadiusChunks * 4);

        int X;
        int Z;
        int index = 0;

        var chunkPos = new List<Vector2Int>(sqrRadiusChunks * 4);
        var chunkIndex = new List<int>(sqrRadiusChunks * 4);

        for (int i = 0; i < 4 * sqrRadiusChunks; i++)
        {
            X = i % (2 * RadiusChunks) + currentPlayerChunk.x - RadiusChunks;
            Z = i / (2 * RadiusChunks) + currentPlayerChunk.y - RadiusChunks;

            if ((new Vector2Int(X, Z) - currentPlayerChunk).sqrMagnitude > sqrRadiusChunksInequality) continue;

            if (ChunksPos.Contains(new Vector2Int(X, Z))) continue;

            ChunkData chunkData = new ChunkData(new Vector2Int(X, Z), ChunkDataState.ToMove, null, index);
            chunkIndex.Add(index);
            
            GenerateMeshChunk(InitChunk(chunkData), chunkPos);            

            index++;           
        }

        var parallelJob = new MeshChunkBuilder.GenerateLodMeshParallel()
        {
            seed = seed,

            OriginalSizeChunk = perlinData.sizeChunk,
            CountOriginalChunksInLengthBatch = CountOriginalChunksInLengthBatch,

            maxHeight = perlinData.maxHeight,

            PerlinCountChunks = perlinData.countChunks,
            settingSideProportion = perlinData.settingSideProportion,
            lengthRowOct = perlinData.LengthRowOctaves,
            oneSetting = perlinData.oneSetting,
            SettingsSize = perlinData.settingsSize,

            CountVerticiesInLodChunk = CountOriginalChunksInLengthBatch * LodDensity + 1,
            SqrCountVerticiesInLodChunk = (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1),
            LodDensity = LodDensity,

            SideSize = perlinData.SideSize,
            waterLevel = perlinData.waterLevel,
            countElementsRiver = perlinData.countElementsRiver,
            startCoordHeight = perlin.startCoordHeight,

            polygonScale = polygonScale,

            oneWithDivider = new NativeArray<float>(perlinData.oneWithDivider, Allocator.Persistent),
            centersWithDivider = new NativeArray<float>(perlinData.centersWithDivider, Allocator.Persistent),
            dividers = new NativeArray<ushort>(perlinData.dividers, Allocator.Persistent),

            TrianglesForNormals = new NativeArray<uint>(NormalsLodTriangles, Allocator.Persistent),
            triangles = new NativeArray<uint>(LodTriangles, Allocator.Persistent),

            LodChunkPos = new NativeArray<Vector2Int>(chunkPos.ToArray(), Allocator.Persistent),
            IndexChunks = new NativeArray<int>(chunkIndex.ToArray(), Allocator.Persistent),

            verticies = new NativeArray<float3>(chunkIndex.Count * (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1), Allocator.Persistent),
            normals = new NativeArray<float3>(chunkIndex.Count * (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1), Allocator.Persistent),
            uvs = new NativeArray<float2>(chunkIndex.Count * (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1), Allocator.Persistent),

            Links = perlinData.NativeLinks,
            SettingsOctaves = perlinData.NativeSettingsOctaves,
            SettingsOctavesSlices = perlinData.NativeSettingsOctaveSlices,
            RiversData = perlinData.NativeRiversData,
            RiversSlices = perlinData.NativeRiversSlices,
            ReliefTypes = perlinData.NativeReliefTypes,            
        };
        ChunkGenerateJobQueue.Enqueue(parallelJob);
        
        var jobHandle = parallelJob.ScheduleParallel(chunkPos.Count, 8, default);

        ChunkGenerateQueue.Enqueue(jobHandle);        
    }

    private ChunkData InitChunk(ChunkData chunk)
    {
        var chunkObject = Instantiate(chunkPrefab, new Vector3(chunk.pos.x * ConvertBatchPosInWorldPos, 0, chunk.pos.y * ConvertBatchPosInWorldPos), Quaternion.identity, transform);        
        chunk.chunk = chunkObject;
        chunk.rend = chunkObject.GetComponent<ChunkRenderer>();
        chunk.rend.SetStartMeshData((CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1), LodTriangles.Length, new Vector3(ConvertBatchPosInWorldPos, perlinData.maxHeight * 1.5f, ConvertBatchPosInWorldPos));
        Chunks.Add(chunk);

        ChunksToMove.Add(chunk.index);      
        
        return chunk;
    }


    private void ChunkGenerate()
    {
        var chunkPos = new List<Vector2Int>(sqrRadiusChunks * 4);
        var chunkIndex = new List<int>(sqrRadiusChunks * 4);

        int X;
        int Z;
        for (int j = 0; j < 4 * sqrRadiusChunks; j++)
        {
            X = j % (2 * RadiusChunks) + currentPlayerChunk.x - RadiusChunks;
            Z = j / (2 * RadiusChunks) + currentPlayerChunk.y - RadiusChunks;

            if ((new Vector2Int(X, Z) - currentPlayerChunk).sqrMagnitude > sqrRadiusChunksInequality) continue;

            if (ChunksPos.Contains(new Vector2Int(X, Z)) || ChunksToMove.Count == 0) continue;
               
            ChunkData chunkData = Chunks[ChunksToMove[ChunksToMove.Count - 1]];
            ChunksPos.Remove(Chunks[chunkData.index].pos);
            chunkData.pos = new Vector2Int(X, Z);

            chunkIndex.Add(chunkData.index);

            GenerateMeshChunk(chunkData, chunkPos);       
        }

        var parallelJob = new MeshChunkBuilder.GenerateLodMeshParallel()
        {
            seed = seed,

            OriginalSizeChunk = perlinData.sizeChunk,
            CountOriginalChunksInLengthBatch = CountOriginalChunksInLengthBatch,

            maxHeight = perlinData.maxHeight,

            PerlinCountChunks = perlinData.countChunks,
            settingSideProportion = perlinData.settingSideProportion,
            lengthRowOct = perlinData.LengthRowOctaves,
            oneSetting = perlinData.oneSetting,
            SettingsSize = perlinData.settingsSize,

            CountVerticiesInLodChunk = CountOriginalChunksInLengthBatch * LodDensity + 1,
            SqrCountVerticiesInLodChunk = (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1),
            LodDensity = LodDensity,

            SideSize = perlinData.SideSize,
            waterLevel = perlinData.waterLevel,
            countElementsRiver = perlinData.countElementsRiver,
            startCoordHeight = perlin.startCoordHeight,

            polygonScale = polygonScale,

            oneWithDivider = new NativeArray<float>(perlinData.oneWithDivider, Allocator.Persistent),
            centersWithDivider = new NativeArray<float>(perlinData.centersWithDivider, Allocator.Persistent),
            dividers = new NativeArray<ushort>(perlinData.dividers, Allocator.Persistent),

            TrianglesForNormals = new NativeArray<uint>(NormalsLodTriangles, Allocator.Persistent),
            triangles = new NativeArray<uint>(LodTriangles, Allocator.Persistent),

            LodChunkPos = new NativeArray<Vector2Int>(chunkPos.ToArray(), Allocator.Persistent),
            IndexChunks = new NativeArray<int>(chunkIndex.ToArray(), Allocator.Persistent),

            verticies = new NativeArray<float3>(chunkIndex.Count * (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1), Allocator.Persistent),
            normals = new NativeArray<float3>(chunkIndex.Count * (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1), Allocator.Persistent),
            uvs = new NativeArray<float2>(chunkIndex.Count * (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1), Allocator.Persistent),

            Links = perlinData.NativeLinks,
            SettingsOctaves = perlinData.NativeSettingsOctaves,
            SettingsOctavesSlices = perlinData.NativeSettingsOctaveSlices,
            RiversData = perlinData.NativeRiversData,
            RiversSlices = perlinData.NativeRiversSlices,
            ReliefTypes = perlinData.NativeReliefTypes,            
        };
        ChunkGenerateJobQueue.Enqueue(parallelJob);
        
        var jobHandle = parallelJob.ScheduleParallel(chunkPos.Count, 8, default);

        jobHandle.Complete();

        ChunkGenerateQueue.Enqueue(jobHandle);        
    }

    private void GenerateMeshChunk(ChunkData chunkData, List<Vector2Int> chunkPos)
    {
        var pos = chunkData.pos;
        chunkData.chunk.transform.position = new Vector3(pos.x * ConvertBatchPosInWorldPos, 0, pos.y * ConvertBatchPosInWorldPos);        
        
        ChunksPos.Add(pos);
        chunkPos.Add(pos);

        chunkData.state = ChunkDataState.StartingLoading;        
        ChunksToMove.RemoveAt(ChunksToMove.Count - 1);
    }

    private Queue<JobHandle> ChunkGenerateQueue;
    private Queue<MeshChunkBuilder.GenerateLodMeshParallel> ChunkGenerateJobQueue;

    private bool isCheckSetGenerate;
    private float DistToChangePosSimulation;    

    private Vector2Int currentPosViewChunks;
    private Vector2 playerPosViewChunks;    
    private void Update()
    {
        CoroutineTimeManager.SetLimitedFrameRate();

        if (isCheckSetGenerate)
        {
            //if (oldViewRadius != viewRadius)
            //{
            //    CreateViewRangeChunks();
            //    Time.timeScale = 0;
            //    StartCoroutine(WaitingEndGenerationChunks());
            //}

            playerChunk.Set(player.transform.position.x / ConvertBatchPosInWorldPos, player.transform.position.z / ConvertBatchPosInWorldPos);
            if (Mathf.Abs(playerChunk.x - currentPlayerChunk.x) > DistToChangePosSimulation || Mathf.Abs(playerChunk.y - currentPlayerChunk.y) > DistToChangePosSimulation)
            {
                currentPlayerChunk.Set((int)Math.Round(playerChunk.x, MidpointRounding.AwayFromZero), (int)Math.Round(playerChunk.y, MidpointRounding.AwayFromZero));
                AddChunksToMove();
            }

            playerPosViewChunks.Set(player.transform.position.x / LodLevels[0].ConvertBatchPosInWorldPos, player.transform.position.z / LodLevels[0].ConvertBatchPosInWorldPos);
            if (Mathf.Abs(playerPosViewChunks.x - currentPosViewChunks.x) > LodLevels[0].DistToChangePosView || Mathf.Abs(playerPosViewChunks.y - currentPosViewChunks.y) > LodLevels[0].DistToChangePosView)
            {
                currentPosViewChunks.Set((int)Math.Round(playerPosViewChunks.x, MidpointRounding.AwayFromZero), (int)Math.Round(playerPosViewChunks.y, MidpointRounding.AwayFromZero));
               
                    for (int i = 0; i < LodLevels.Length; i++)
                    {
                        LodLevels[i].currentPlayerChunk = currentPosViewChunks;
                        StartCoroutine(LodLevels[i].AddChunksToMove());
                    }
            }
        }
    }

    [SerializeField] private Mesh LodWater;
    [SerializeField] private Material WaterMaterial;
    [SerializeField] private int WaterLayer;
        

    private MeshChunkBuilder.GenerateLodMeshParallel CurrentJob;
    private void LateUpdate()
    {
        if (isCheckSetGenerate)
        {            
            if (ChunkGenerateQueue.Count > 0 && ChunkGenerateQueue.Peek().IsCompleted)
            {   
                CurrentJob.DisposeTempData();
                CurrentJob.DisposeMainData();

                CurrentJob = ChunkGenerateJobQueue.Peek();
                SetMeshesChunks();                  
            }            
            Graphics.DrawMesh(LodWater, Matrix4x4.TRS(new Vector3(currentPosViewChunks.x * LodLevels[0].ConvertBatchPosInWorldPos, perlinData.heightWaterLevel, currentPosViewChunks.y * LodLevels[0].ConvertBatchPosInWorldPos)
                , Quaternion.Euler(new Vector3(-90f, 0, 0)), new Vector3(LodLevels[LodLevels.Length - 1].RadiusLodBatches * LodLevels[0].ConvertBatchPosInWorldPos, LodLevels[LodLevels.Length - 1].RadiusLodBatches * LodLevels[0].ConvertBatchPosInWorldPos, 1)), WaterMaterial, WaterLayer);

            for (int i = 0; i < LodLevels.Length; i++)
            {
                if (LodLevels[i].ChunkGenerateQueue.Count > 0 && LodLevels[i].ChunkGenerateQueue.Peek().IsCompleted)
                {   
                    StopCoroutine(LodLevels[i].SetBatchesMesh());
                    LodLevels[i].CurrentJob.DisposeTempData();
                    LodLevels[i].CurrentJob.DisposeMainData();

                    LodLevels[i].CurrentJob = LodLevels[i].ChunkGenerateJobQueue.Peek();
                    StartCoroutine(LodLevels[i].SetBatchesMesh());                    
                }
            }
        }
    }
    
    private void SetMeshesChunks()
    {
        IsChunksGenerated = false;

        var handle = ChunkGenerateQueue.Dequeue();

        handle.Complete();

        var parallelJob = ChunkGenerateJobQueue.Dequeue();        
        parallelJob.DisposeTempData();
        CurrentJob = parallelJob;
        for (int i = 0; i < parallelJob.LodChunkPos.Length; i++)
        {                                                
            Vector2Int chunkPos = parallelJob.LodChunkPos[i];
            int chunkIndex = parallelJob.IndexChunks[i];

            var chunk = Chunks[chunkIndex];

            chunk.chunk.transform.position = new Vector3(chunkPos.x * ConvertBatchPosInWorldPos, 0, chunkPos.y * ConvertBatchPosInWorldPos);
            chunk.state = ChunkDataState.Loaded;
                        
            chunk.rend.chunkMesh.SetVertexBufferData<float3>(parallelJob.verticies, i * parallelJob.SqrCountVerticiesInLodChunk, 0, parallelJob.SqrCountVerticiesInLodChunk, 0, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);
            chunk.rend.chunkMesh.SetVertexBufferData<float3>(parallelJob.normals, i * parallelJob.SqrCountVerticiesInLodChunk, 0, parallelJob.SqrCountVerticiesInLodChunk, 1, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);
            chunk.rend.chunkMesh.SetVertexBufferData<float2>(parallelJob.uvs, i * parallelJob.SqrCountVerticiesInLodChunk, 0, parallelJob.SqrCountVerticiesInLodChunk, 2, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);
            
            chunk.rend.chunkMesh.SetIndexBufferData<uint>(LodTriangles, 0, 0, LodTriangles.Length, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);

            chunk.rend.UpdateMesh();            
        }
        parallelJob.DisposeMainData();
        CurrentJob = parallelJob;

        IsChunksGenerated = true;
    }    

    private void AddChunksToMove()
    {
        for (int i = 0; i < Chunks.Count; i++)
        {
            var chunk = Chunks[i];
            // Проверяем квадрат расстояния для удаления чанкa
            if (chunk.state != ChunkDataState.ToMove && (chunk.pos - currentPlayerChunk).sqrMagnitude > sqrRadiusChunksInequality)
            {
                ChunksToMove.Add(chunk.index);                
                chunk.state = ChunkDataState.ToMove;
            }            
        }
        ChunkGenerate();
    }

    void OnApplicationQuit()
    {
        SaveSystem.SaveData(seed, "Seed");
        SaveSystem.SaveData(oldSeed, "OldSeed");
        if (perlinData != null) 
            perlinData.SaveBaseConstantData();        
        
        if (ChunkGenerateQueue != null)
        while (ChunkGenerateQueue.Count > 0)
        {
            ChunkGenerateQueue.Dequeue().Complete();           
        }

        if(ChunkGenerateJobQueue != null)
        while (ChunkGenerateJobQueue.Count > 0)
        {
            var job = ChunkGenerateJobQueue.Dequeue();
            job.DisposeTempData();
            job.DisposeMainData();            
        }
        if (perlin != null)
            perlin.DisposeNativeBigBaseData();
    }
}