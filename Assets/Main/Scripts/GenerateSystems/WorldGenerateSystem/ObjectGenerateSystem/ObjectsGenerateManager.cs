using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static LODChunksManager;

public class ObjectsGenerateManager : MonoBehaviour
{
    [SerializeField] private PerlinNoise perlin;
    [SerializeField] private PerlinNoiseData perlinData;

    [SerializeField] private GameObject player;
    [SerializeField] private PlayerData playerData;

    [SerializeField] private GameObject Cabin;
    [SerializeField] private PlayerCabinData CabinData;
        
    

    public void SetBaseObjectsInStart(bool isNewGame)
    {       
        if(isNewGame)
        {
            Cabin.transform.position = new Vector3(0f, perlin.GetPointHeight(0, 0, 0, 0) * perlinData.maxHeight, 0f);
            Cabin.isStatic = true;
            player.transform.position = new Vector3(0f, perlin.GetPointHeight(0, 0, 0, 0) * perlinData.maxHeight + 1, 0f);
        }
        else
        {
            SaveSystem.SavedData<Vector3> dataPlayerPos = SaveSystem.LoadData<Vector3>(playerData.nameFilePlayerPos);
            if (dataPlayerPos != null)
            {
                Vector3 playerStartPos = dataPlayerPos.data;
                player.transform.position = playerStartPos;
            }
            else
            {
                player.transform.position = new Vector3(0f, perlin.GetPointHeight(0, 0, 0, 0) * perlinData.maxHeight + 1f, 0f);
            }

            SaveSystem.SavedData<Vector3> dataCabinPos = SaveSystem.LoadData<Vector3>(CabinData.nameFilePos);
            SaveSystem.SavedData<Quaternion> dataCabinRot = SaveSystem.LoadData<Quaternion>(CabinData.nameFileRot);
            if (dataCabinPos != null)
            {
                Cabin.transform.position = dataCabinPos.data;
                Cabin.transform.rotation = dataCabinRot.data;
            }
            else
            {
                Cabin.transform.position = new Vector3(0f, perlin.GetPointHeight(0, 0, 0, 0) * perlinData.maxHeight, 0f);
            }
            Cabin.isStatic = true;
        }
    }

    //public void CreateViewRangeChunks()
    //{
    //    StopCoroutine(ChunkGenerate());
    //    StopCoroutine(AddChunksToMove());
    //    StopCoroutine(SetBatchesMesh());

    //    InitMainGenData();

    //    perlin = GetComponent<PerlinNoise>();
    //    perlinData = GetComponent<ChunkGenerator>().perlinData;

    //    ConvertBatchPosInWorldPos = CountOriginalChunksInLengthBatch * (perlinData.sizeChunk - 1) * polygonScale;
    //    sqrViewRadiusBatches = RadiusLodBatches * RadiusLodBatches;
    //    sqrViewRadiusBatchesInequality = (RadiusLodBatches) * (RadiusLodBatches);

    //    DistToChangePosView = 0.5f + (float)(perlinData.sizeChunk - 1) / ConvertBatchPosInWorldPos;

    //    SqrDistToDontDraw = (RadiusToDontDrawBatches) * (RadiusToDontDrawBatches);
    //    CountBatches = 4 * sqrViewRadiusBatches;

    //    ChunkGenerateQueue = new Queue<JobHandle>(CountBatches);
    //    ChunkGenerateJobQueue = new Queue<MeshChunkBuilder.GenerateLodMeshParallel>(CountBatches);

    //    LodChunks = new List<LodChunkData>(CountBatches);
    //    ChunksToMove = new List<int>(CountBatches);
    //    ChunksPos = new HashSet<Vector2Int>(CountBatches);

    //    int X;
    //    int Z;
    //    int index = 0;

    //    var chunkPos = new List<Vector2Int>(CountBatches);
    //    var chunkIndex = new List<int>(CountBatches);

    //    int dist;
    //    Vector2Int localPos;
    //    for (int i = 0; i < 4 * sqrViewRadiusBatches; i++)
    //    {
    //        X = i % (2 * RadiusLodBatches) + currentPlayerChunk.x - RadiusLodBatches;
    //        Z = i / (2 * RadiusLodBatches) + currentPlayerChunk.y - RadiusLodBatches;

    //        localPos = new Vector2Int(X, Z) - currentPlayerChunk;
    //        if ((localPos.x == 0 && Mathf.Abs(localPos.y) == RadiusLodBatches) || (localPos.y == 0 && Mathf.Abs(localPos.x) == RadiusLodBatches)) continue;

    //        dist = localPos.sqrMagnitude;
    //        if (dist > sqrViewRadiusBatchesInequality) continue;

    //        if (ChunksPos.Contains(new Vector2Int(X, Z))) continue;

    //        LodChunkData chunkData = new LodChunkData(new Vector2Int(X, Z), LodChunkState.ToMove, new Mesh(), index, 0);

    //        chunkIndex.Add(index);

    //        SetDataChunkForMeshGenerate(InitChunk(chunkData), chunkPos);

    //        index++;
    //    }

    //    var parallelJob = new MeshChunkBuilder.GenerateLodMeshParallel()
    //    {
    //        seed = seed,

    //        OriginalSizeChunk = perlinData.sizeChunk,
    //        CountOriginalChunksInLengthBatch = CountOriginalChunksInLengthBatch,

    //        maxHeight = perlinData.maxHeight,

    //        PerlinCountChunks = perlinData.countChunks,
    //        settingSideProportion = perlinData.settingSideProportion,
    //        lengthRowOct = perlinData.LengthRowOctaves,
    //        oneSetting = perlinData.oneSetting,
    //        SettingsSize = perlinData.settingsSize,

    //        CountVerticiesInLodChunk = CountOriginalChunksInLengthBatch * LodDensity + 1,
    //        SqrCountVerticiesInLodChunk = (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1),
    //        LodDensity = LodDensity,

    //        SideSize = perlinData.SideSize,
    //        waterLevel = perlinData.waterLevel,
    //        countElementsRiver = perlinData.countElementsRiver,
    //        startCoordHeight = perlin.startCoordHeight,

    //        polygonScale = polygonScale,

    //        oneWithDivider = new NativeArray<float>(perlinData.oneWithDivider, Allocator.Persistent),
    //        centersWithDivider = new NativeArray<float>(perlinData.centersWithDivider, Allocator.Persistent),
    //        dividers = new NativeArray<ushort>(perlinData.dividers, Allocator.Persistent),

    //        TrianglesForNormals = new NativeArray<uint>(NormalsLodTriangles, Allocator.Persistent),
    //        triangles = new NativeArray<uint>(LodTriangles, Allocator.Persistent),

    //        LodChunkPos = new NativeArray<Vector2Int>(chunkPos.ToArray(), Allocator.Persistent),
    //        IndexChunks = new NativeArray<int>(chunkIndex.ToArray(), Allocator.Persistent),

    //        verticies = new NativeArray<float3>(chunkIndex.Count * (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1), Allocator.Persistent),
    //        normals = new NativeArray<float3>(chunkIndex.Count * (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1), Allocator.Persistent),
    //        uvs = new NativeArray<float2>(chunkIndex.Count * (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1), Allocator.Persistent),

    //        Links = perlinData.NativeLinks,
    //        SettingsOctaves = perlinData.NativeSettingsOctaves,
    //        SettingsOctavesSlices = perlinData.NativeSettingsOctaveSlices,
    //        RiversData = perlinData.NativeRiversData,
    //        RiversSlices = perlinData.NativeRiversSlices,
    //        ReliefTypes = perlinData.NativeReliefTypes,

    //        IsMainDataDisposed = new NativeReference<bool>(false, Allocator.Persistent)
    //    };
    //    ChunkGenerateJobQueue.Enqueue(parallelJob);
    //    var jobHandle = parallelJob.ScheduleParallel(chunkPos.Count, 8, default);

    //    ChunkGenerateQueue.Enqueue(jobHandle);
    //}

    //private LodChunkData InitChunk(LodChunkData chunk)
    //{
    //    LodChunks.Add(chunk);
    //    ChunksToMove.Add(chunk.index);

    //    int CountVerticies = (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1);

    //    chunk.mesh.SetVertexBufferParams(CountVerticies,
    //    new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, stream: 0),
    //    new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, stream: 1),
    //    new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, stream: 2));

    //    chunk.mesh.SetIndexBufferParams(LodTriangles.Length, IndexFormat.UInt32);

    //    var size = new Vector3(ConvertBatchPosInWorldPos, perlinData.maxHeight * 1.5f, ConvertBatchPosInWorldPos);
    //    chunk.mesh.bounds = new Bounds(new Vector3(0, size.y * 0.5f, 0), size);

    //    chunk.mesh.subMeshCount = 1;
    //    chunk.mesh.SetSubMesh(0, new SubMeshDescriptor(0, LodTriangles.Length)
    //    {
    //        vertexCount = CountVerticies,
    //    }, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);

    //    return chunk;
    //}

    //private IEnumerator ChunkGenerate()
    //{
    //    var chunkPos = new List<Vector2Int>(CountBatches);
    //    var chunkIndex = new List<int>(CountBatches);

    //    int X;
    //    int Z;
    //    int dist;
    //    Vector2Int localPos;
    //    for (int i = 0; i < 4 * sqrViewRadiusBatches; i++)
    //    {
    //        X = i % (2 * RadiusLodBatches) + currentPlayerChunk.x - RadiusLodBatches;
    //        Z = i / (2 * RadiusLodBatches) + currentPlayerChunk.y - RadiusLodBatches;

    //        localPos = new Vector2Int(X, Z) - currentPlayerChunk;
    //        if ((localPos.x == 0 && Mathf.Abs(localPos.y) == RadiusLodBatches) || (localPos.y == 0 && Mathf.Abs(localPos.x) == RadiusLodBatches)) continue;

    //        dist = localPos.sqrMagnitude;
    //        if (dist > sqrViewRadiusBatchesInequality) continue;

    //        if (ChunksPos.Contains(new Vector2Int(X, Z)) || ChunksToMove.Count == 0) continue;

    //        LodChunkData chunkData = LodChunks[ChunksToMove[ChunksToMove.Count - 1]];
    //        ChunksPos.Remove(LodChunks[chunkData.index].LodChunkPos);
    //        chunkData.LodChunkPos = new Vector2Int(X, Z);

    //        chunkIndex.Add(chunkData.index);

    //        SetDataChunkForMeshGenerate(chunkData, chunkPos);
    //        if (CoroutineTimeManager.GetCoroutineBreakMoment()) yield return null;
    //    }

    //    var parallelJob = new MeshChunkBuilder.GenerateLodMeshParallel()
    //    {
    //        seed = seed,

    //        OriginalSizeChunk = perlinData.sizeChunk,
    //        CountOriginalChunksInLengthBatch = CountOriginalChunksInLengthBatch,

    //        maxHeight = perlinData.maxHeight,

    //        PerlinCountChunks = perlinData.countChunks,
    //        settingSideProportion = perlinData.settingSideProportion,
    //        lengthRowOct = perlinData.LengthRowOctaves,
    //        oneSetting = perlinData.oneSetting,
    //        SettingsSize = perlinData.settingsSize,

    //        CountVerticiesInLodChunk = CountOriginalChunksInLengthBatch * LodDensity + 1,
    //        SqrCountVerticiesInLodChunk = (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1),
    //        LodDensity = LodDensity,

    //        SideSize = perlinData.SideSize,
    //        waterLevel = perlinData.waterLevel,
    //        countElementsRiver = perlinData.countElementsRiver,
    //        startCoordHeight = perlin.startCoordHeight,

    //        polygonScale = polygonScale,

    //        oneWithDivider = new NativeArray<float>(perlinData.oneWithDivider, Allocator.Persistent),
    //        centersWithDivider = new NativeArray<float>(perlinData.centersWithDivider, Allocator.Persistent),
    //        dividers = new NativeArray<ushort>(perlinData.dividers, Allocator.Persistent),

    //        TrianglesForNormals = new NativeArray<uint>(NormalsLodTriangles, Allocator.Persistent),
    //        triangles = new NativeArray<uint>(LodTriangles, Allocator.Persistent),

    //        LodChunkPos = new NativeArray<Vector2Int>(chunkPos.ToArray(), Allocator.Persistent),
    //        IndexChunks = new NativeArray<int>(chunkIndex.ToArray(), Allocator.Persistent),

    //        verticies = new NativeArray<float3>(chunkIndex.Count * (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1), Allocator.Persistent),
    //        normals = new NativeArray<float3>(chunkIndex.Count * (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1), Allocator.Persistent),
    //        uvs = new NativeArray<float2>(chunkIndex.Count * (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1), Allocator.Persistent),

    //        Links = perlinData.NativeLinks,
    //        SettingsOctaves = perlinData.NativeSettingsOctaves,
    //        SettingsOctavesSlices = perlinData.NativeSettingsOctaveSlices,
    //        RiversData = perlinData.NativeRiversData,
    //        RiversSlices = perlinData.NativeRiversSlices,
    //        ReliefTypes = perlinData.NativeReliefTypes,

    //        IsMainDataDisposed = new NativeReference<bool>(false, Allocator.Persistent)
    //    };
    //    ChunkGenerateJobQueue.Enqueue(parallelJob);
    //    var jobHandle = parallelJob.ScheduleParallel(chunkPos.Count, 8, default);

    //    ChunkGenerateQueue.Enqueue(jobHandle);
    //}

    //[HideInInspector] public MeshChunkBuilder.GenerateLodMeshParallel CurrentJob;

    //private void SetDataChunkForMeshGenerate(LodChunkData chunkData, List<Vector2Int> chunkPos)
    //{
    //    var pos = chunkData.LodChunkPos;

    //    ChunksPos.Add(pos);
    //    chunkPos.Add(pos);

    //    chunkData.state = LodChunkState.NotToMove;
    //    ChunksToMove.RemoveAt(ChunksToMove.Count - 1);
    //}

    //[SerializeField] private GameObject player;
    //private Vector2 playerChunk;
    //[HideInInspector] public bool isCheckSetGenerate;

    //public float ConvertBatchPosInWorldPos { private set; get; }
    //[SerializeField] private Material ChunkMaterial;
    //[SerializeField] private int LodChunkLayer;

    //private float SqrDistToDraw;
    //private void LateUpdate()
    //{
    //    if (isCheckSetGenerate)
    //    {
    //        if (LodChunks != null && LodChunks.Count > 0)
    //        {
    //            for (int i = 0; i < LodChunks.Count; i++)
    //            {
    //                SqrDistToDraw = (LodChunks[i].LodChunkPos - currentPlayerChunk).sqrMagnitude;
    //                if (LodChunks[i].mesh == null || SqrDistToDraw < SqrDistToDontDraw) continue;
    //                Graphics.DrawMesh(LodChunks[i].mesh, Matrix4x4.TRS(LodChunks[i].WorldPos, Quaternion.identity, Vector3.one), ChunkMaterial, LodChunkLayer);
    //            }
    //        }
    //    }
    //}

    //[HideInInspector] public bool IsBatchesGenerated;
    //public IEnumerator SetBatchesMesh()
    //{
    //    IsBatchesGenerated = false;
    //    var handle = ChunkGenerateQueue.Dequeue();

    //    handle.Complete();

    //    var parallelJob = ChunkGenerateJobQueue.Dequeue();
    //    parallelJob.DisposeTempData();

    //    int indexChunk;
    //    for (int i = 0; i < parallelJob.LodChunkPos.Length; i++)
    //    {
    //        indexChunk = parallelJob.IndexChunks[i];
    //        LodChunks[indexChunk].WorldPos = new Vector3(parallelJob.LodChunkPos[i].x * ConvertBatchPosInWorldPos, 0, parallelJob.LodChunkPos[i].y * ConvertBatchPosInWorldPos);
    //        LodChunks[indexChunk].state = LodChunkState.NotToMove;

    //        LodChunks[indexChunk].mesh.SetVertexBufferData<float3>(parallelJob.verticies, i * parallelJob.SqrCountVerticiesInLodChunk, 0, parallelJob.SqrCountVerticiesInLodChunk, 0, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);
    //        LodChunks[indexChunk].mesh.SetVertexBufferData<float3>(parallelJob.normals, i * parallelJob.SqrCountVerticiesInLodChunk, 0, parallelJob.SqrCountVerticiesInLodChunk, 1, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);
    //        LodChunks[indexChunk].mesh.SetVertexBufferData<float2>(parallelJob.uvs, i * parallelJob.SqrCountVerticiesInLodChunk, 0, parallelJob.SqrCountVerticiesInLodChunk, 2, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);

    //        LodChunks[indexChunk].mesh.SetIndexBufferData<uint>(LodTriangles, 0, 0, LodTriangles.Length, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);

    //        if (CoroutineTimeManager.GetCoroutineBreakMoment()) yield return null;
    //    }
    //    parallelJob.DisposeMainData();

    //    IsBatchesGenerated = true;
    //}

    //public IEnumerator AddChunksToMove()
    //{
    //    Vector2Int localPos;
    //    int dist;
    //    for (int i = 0; i < LodChunks.Count; i++)
    //    {
    //        var chunk = LodChunks[i];

    //        localPos = chunk.LodChunkPos - currentPlayerChunk;

    //        dist = localPos.sqrMagnitude;
    //        // Проверяем квадрат расстояния для удаления чанкa
    //        if (chunk.state != LodChunkState.ToMove && (dist > sqrViewRadiusBatchesInequality || (localPos.x == 0 && Mathf.Abs(localPos.y) == RadiusLodBatches) || (localPos.y == 0 && Mathf.Abs(localPos.x) == RadiusLodBatches)))
    //        {
    //            ChunksToMove.Add(chunk.index);
    //            chunk.state = LodChunkState.ToMove;
    //        }
    //        if (CoroutineTimeManager.GetCoroutineBreakMoment()) yield return null;
    //    }

    //    StartCoroutine(ChunkGenerate());
    //}

    //void OnApplicationQuit()
    //{
    //    if (ChunkGenerateQueue != null)
    //        while (ChunkGenerateQueue.Count > 0)
    //        {
    //            ChunkGenerateQueue.Dequeue().Complete();
    //        }

    //    if (ChunkGenerateJobQueue != null)
    //        while (ChunkGenerateJobQueue.Count > 0)
    //        {
    //            var job = ChunkGenerateJobQueue.Dequeue();
    //            job.DisposeTempData();
    //            job.DisposeMainData();
    //            job.GetIsMainDataDisposed();
    //        }
    //}

    //[BurstCompile]
    //public struct GenerateObjDatasJob : IJobFor
    //{
    //    public int seed;

    //    public float maxHeight;

    //    public int CountOriginalChunksInLengthBatch;
    //    public int SqrCountOriginalChunksInLengthBatch;
    //    public byte ObjChunkSize;
    //    public byte OriginalSizeChunk;           

    //    public ushort SideSize;
    //    public int SettingsSize;
    //    public ushort PerlinCountChunks;

    //    public byte settingSideProportion;
    //    public float oneSetting;
    //    public int lengthRowOct;

    //    public float waterLevel;
    //    public int countElementsRiver;

    //    public float startCoordHeight;

    //    public float polygonScale;

    //    [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<float> oneWithDivider;
    //    [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<float> centersWithDivider;
    //    [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<ushort> dividers;

    //    [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<float> Links;

    //    [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<byte> SettingsOctaves;
    //    [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<int2> SettingsOctavesSlices;

    //    [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<float> RiversData;
    //    [NativeDisableParallelForRestriction][ReadOnly] public NativeParallelHashMap<int2, int2> RiversSlices;
    //    [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<int> ReliefTypes;

    //    [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<uint> triangles;
    //    [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<uint> TrianglesForNormals;

    //    [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<Vector2Int> LodChunkPos;
    //    [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<int> IndexChunks;

    //    [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<Matrix4x4> TRSObjects;

    //    public uint MinObjectCountInChunk;
    //    public uint MaxObjectCountInChunk;
    //    public float MinObjPosHeightFromWaterLv;
    //    public int ObjectsID;
        
    //    public void Execute(int startIndex)
    //    {
    //        GenerateMesh(startIndex);
    //    }

    //    private void GenerateMesh(int LodChunkIndex)
    //    {            
    //        var random = new Unity.Mathematics.Random();
    //        for (int i = 0; i < SqrCountOriginalChunksInLengthBatch; i++)
    //        {
    //            var ChunkPos = new Vector2Int(LodChunkPos[LodChunkIndex].x * CountOriginalChunksInLengthBatch + i % CountOriginalChunksInLengthBatch, LodChunkPos[LodChunkIndex].y * CountOriginalChunksInLengthBatch + i / CountOriginalChunksInLengthBatch);
    //            var ChunkHash = math.hash(new int4(ChunkPos.x, ChunkPos.y, seed, ObjectsID));
    //            random.state = ChunkHash;
    //            var countInChunk = random.NextUInt(MinObjectCountInChunk, MaxObjectCountInChunk + 1);
    //            for (uint j = 0; j < MaxObjectCountInChunk; j++)
    //            {
    //                var x = random.NextFloat(0f, ObjChunkSize) + ChunkPos.x * ObjChunkSize;
    //                var z = random.NextFloat(0f, ObjChunkSize) + ChunkPos.y * ObjChunkSize;
    //                var y = PerlinNoise.GetPointHeightNative(x, z, settingSideProportion, oneSetting, OriginalSizeChunk, PerlinCountChunks, SideSize, waterLevel,
    //                countElementsRiver, oneWithDivider, centersWithDivider, dividers, Links, SettingsOctaves, SettingsOctavesSlices, SettingsSize, RiversData, RiversSlices, ReliefTypes, seed, maxHeight, startCoordHeight);
    //                if (j < countInChunk && y > waterLevel * maxHeight + MinObjPosHeightFromWaterLv)
    //                {                         
    //                    TRSObjects[(int)(LodChunkIndex * MaxObjectCountInChunk * MaxObjectCountInChunk + i * MaxObjectCountInChunk + j)].SetTRS(new Vector3(x, y, z), Quaternion.identity * Quaternion.Euler(0f, random.NextFloat(0f, 360f), 0f), Vector3.one);
    //                }
    //                else
    //                    TRSObjects[(int)(LodChunkIndex * MaxObjectCountInChunk * MaxObjectCountInChunk + i * MaxObjectCountInChunk + j)].SetTRS(new Vector3(x, -1000f, z), Quaternion.identity, Vector3.one);
    //            }
    //        }            
    //    }        

    //    public void DisposeTempData()
    //    {
    //        if (oneWithDivider.IsCreated) oneWithDivider.Dispose(); if (centersWithDivider.IsCreated) centersWithDivider.Dispose(); if (dividers.IsCreated) dividers.Dispose();
    //        if (triangles.IsCreated) triangles.Dispose(); if (TrianglesForNormals.IsCreated) TrianglesForNormals.Dispose();
    //    }

    //    [NativeDisableParallelForRestriction] public NativeReference<bool> IsMainDataDisposed;
    //    public void DisposeMainData()
    //    {
    //        if (LodChunkPos.IsCreated) LodChunkPos.Dispose(); if (IndexChunks.IsCreated) IndexChunks.Dispose(); if (TRSObjects.IsCreated) TRSObjects.Dispose();
    //        if (IsMainDataDisposed.IsCreated) IsMainDataDisposed.Value = true;
    //    }
    //    public bool GetIsMainDataDisposed()
    //    {
    //        bool value = false;
    //        if (IsMainDataDisposed.IsCreated)
    //        {
    //            value = IsMainDataDisposed.Value;
    //            IsMainDataDisposed.Dispose();
    //        }
    //        return value;
    //    }
    //}
}
